using Ionic.Zip;
using Microsoft.Data.SqlClient; // Changed from MySql.Data.MySqlClient
using MicroERP.API.Helper;
using MicroERP.Shared.Helper;
using MicroERP.Shared.Models;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace MicroERP.API.Services.Services;

public interface IBackupService
{
    Task<BackupResult> BackupSqlServerDatabaseAsync(); // Renamed from BackupMySqlDatabaseAsync
    Task<BackupResult> BackupImagesFolderAsync();
    Task<BackupResult> BackupAllAsync(bool Email, bool FTP);
}

public class BackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private static string DBConnection = string.Empty;
    private readonly Config cfg = new();

    public BackupService()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false);
        _ = builder.Build();

        IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

        _configuration = configuration;

        DBConnection = configuration.GetValue<string>("ConnectionStrings:Default") ?? string.Empty;

        if (!DBConnection.Contains("Pooling"))
            DBConnection += ";Pooling=true;Min Pool Size=5;Max Pool Size=50;Connection Lifetime=300;";
    }

    // Function 1: Backup SQL Server Database using direct SQL connection
    public async Task<BackupResult> BackupSqlServerDatabaseAsync()
    {
        try
        {
            var backupBasePath = await SystemConfigurationHelper.GetConfigValueAsync("Backup", "BasePath") ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups");
            
            if (!Directory.Exists(backupBasePath))
            {
                Directory.CreateDirectory(backupBasePath);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFolder = Path.Combine(backupBasePath, $"DB_Backup_{timestamp}");
            Directory.CreateDirectory(backupFolder);

            var connectionStringBuilder = new SqlConnectionStringBuilder(DBConnection);
            var databaseName = connectionStringBuilder.InitialCatalog;
            var backupFile = Path.Combine(backupFolder, $"{databaseName}_{timestamp}.bak");

            // Use direct SQL connection for backup (more reliable than sqlcmd)
            // Connect to master database to perform backup
            var masterConnectionString = new SqlConnectionStringBuilder(DBConnection)
            {
                InitialCatalog = "master"
            };

            using var connection = new SqlConnection(masterConnectionString.ConnectionString);
            await connection.OpenAsync();

            // Escape the backup file path for SQL (replace single quotes with double single quotes)
            var escapedBackupFile = backupFile.Replace("'", "''");
            
            // Build the backup command
            var backupCommand = $@"
                BACKUP DATABASE [{databaseName}] 
                TO DISK = '{escapedBackupFile}' 
                WITH FORMAT, 
                     MEDIANAME = 'SQLServerBackup', 
                     NAME = 'Full Backup of {databaseName}',
                     STATS = 10";

            using var command = new SqlCommand(backupCommand, connection);
            command.CommandTimeout = 3600; // 1 hour timeout for large databases

            Log.Information("Starting SQL Server backup: {Database} to {BackupFile}", databaseName, backupFile);
            
            await command.ExecuteNonQueryAsync();

            // Verify backup file was created
            if (File.Exists(backupFile))
            {
                var fileInfo = new FileInfo(backupFile);
                Log.Information("SQL Server database backup created successfully: {BackupFile}, Size: {Size} bytes", backupFile, fileInfo.Length);
                return new BackupResult
                {
                    Success = true,
                    BackupPath = backupFile,
                    Message = "Database backup completed successfully",
                    Timestamp = timestamp
                };
            }
            else
            {
                Log.Error("Backup file was not created: {BackupFile}", backupFile);
                return new BackupResult
                {
                    Success = false,
                    Error = "Backup file was not created",
                    Message = "Database backup failed - file not found"
                };
            }
        }
        catch (SqlException sqlEx)
        {
            Log.Error(sqlEx, "SQL Server backup failed: {ErrorNumber} - {Message}", sqlEx.Number, sqlEx.Message);
            return new BackupResult
            {
                Success = false,
                Error = $"SQL Server error: {sqlEx.Message}",
                Message = "Database backup failed"
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating SQL Server database backup");
            return new BackupResult
            {
                Success = false,
                Error = ex.Message,
                Message = "Database backup failed"
            };
        }
    }

    // Function 2: Backup Images Folder (unchanged)
    public async Task<BackupResult> BackupImagesFolderAsync()
    {
        try
        {
            var backupBasePath = await SystemConfigurationHelper.GetConfigValueAsync("Backup", "BasePath") ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups");
            
            if (!Directory.Exists(backupBasePath))
            {
                Directory.CreateDirectory(backupBasePath);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFolder = Path.Combine(backupBasePath, $"Images_Backup_{timestamp}");
            Directory.CreateDirectory(backupFolder);

            var imagesFolder = await SystemConfigurationHelper.GetConfigValueAsync("Images", "FolderPath") ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(imagesFolder))
            {
                Log.Warning("Images folder not found: {ImagesFolder}", imagesFolder);
                return new BackupResult
                {
                    Success = false,
                    Error = "Images folder not found",
                    Message = "Images backup failed - folder not found"
                };
            }

            var imagesBackupFile = Path.Combine(backupFolder, $"images_{timestamp}.zip");

            // Create zip of images folder
            await Task.Run(() => System.IO.Compression.ZipFile.CreateFromDirectory(imagesFolder, imagesBackupFile, CompressionLevel.Optimal, false));

            Log.Information("Images folder backup created: {BackupFile}", imagesBackupFile);

            return new BackupResult
            {
                Success = true,
                BackupPath = imagesBackupFile,
                Message = "Images backup completed successfully",
                Timestamp = timestamp
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error backing up images folder");
            return new BackupResult
            {
                Success = false,
                Error = ex.Message,
                Message = "Images backup failed"
            };
        }
    }

    // Function 3: Backup Both Database and Images with password-protected final compression
    public async Task<BackupResult> BackupAllAsync(bool Email = false, bool FTP = false)
    {
        try
        {
            // Get values from database
            var backupBasePath = await SystemConfigurationHelper.GetConfigValueAsync("Backup", "BasePath") ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups");
            var baseFilePrefix = await SystemConfigurationHelper.GetConfigValueAsync("Backup", "BaseFilePrefix") ?? "UHU";
            var zipPassword = await SystemConfigurationHelper.GetConfigValueAsync("Backup", "ZipPassword");
            var toEmail = await SystemConfigurationHelper.GetConfigValueAsync("Backup", "BackupEmail");
            var imagesFolderPath = await SystemConfigurationHelper.GetConfigValueAsync("Images", "FolderPath") ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            var ftpUrl = await SystemConfigurationHelper.GetConfigValueAsync("FTP", "ServerUrl");
            var ftpUsername = await SystemConfigurationHelper.GetConfigValueAsync("FTP", "Username");
            var ftpPassword = await SystemConfigurationHelper.GetConfigValueAsync("FTP", "Password");

            if (!Directory.Exists(backupBasePath))
            {
                Directory.CreateDirectory(backupBasePath);
            }


            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            // For SQL Server backup, use the base path directly (no nested folders)
            // SQL Server cannot create nested directories, so we'll backup directly to the base path
            // Then create a folder structure locally for organizing the final zip
            var backupFolder = Path.Combine(backupBasePath, $"Full_Backup_{timestamp}");
            Directory.CreateDirectory(backupFolder);

            var results = new List<BackupResult>();

            // Backup database directly to base path (SQL Server can only write to existing directories)
            // Use base path directly, not nested folder, for SQL Server backup
            var dbResult = await BackupSqlServerDatabaseToFolderAsync(backupBasePath, timestamp);
            results.Add(dbResult);

            // Backup images directly into the full backup folder
            var imagesResult = await BackupImagesFolderToFolderAsync(backupFolder, timestamp);
            results.Add(imagesResult);

            // Check if both operations were successful
            var allSuccess = results.All(r => r.Success);

            if (allSuccess)
            {
                // Move backup files to the organized folder structure
                // The database backup is in backupBasePath, move it to backupFolder
                var dbBackupFile = dbResult.BackupPath;
                if (!string.IsNullOrEmpty(dbBackupFile) && File.Exists(dbBackupFile))
                {
                    var dbBackupFileName = Path.GetFileName(dbBackupFile);
                    var dbBackupDestination = Path.Combine(backupFolder, dbBackupFileName);
                    File.Move(dbBackupFile, dbBackupDestination);
                    dbResult.BackupPath = dbBackupDestination;
                    Log.Information("Moved database backup to organized folder: {Destination}", dbBackupDestination);
                }
                
                // Create final password-protected zip file containing both backups
                var finalZipPath = Path.Combine(backupBasePath, $"{baseFilePrefix}_Backup_{timestamp}.backup");

                if (string.IsNullOrEmpty(zipPassword))
                {
                    throw new Exception("Backup password (ZipPassword) is not configured in System Configuration table");
                }

                // Create password-protected zip
                await CreatePasswordProtectedZip(backupFolder, finalZipPath, zipPassword);

                // Clean up the temporary backup folder
                Directory.Delete(backupFolder, true);

                Log.Information("Password-protected full backup created: {BackupFile}", finalZipPath);

                try
                {
                    // Send email with backup
                    bool emailSent = false;
                    if (Email)
                    {
                        if (!string.IsNullOrWhiteSpace(toEmail))
                        {
                            string fileName = Path.GetFileName(finalZipPath);
                            string backupDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                            string subject = $"Backup: {fileName}";

                            string body = $@"
                                            <html>
                                            <body style='font-family:Segoe UI, Tahoma, Geneva, Verdana, sans-serif; color:#333;'>
                                                <h2 style='color:#1E90FF;'>Backup Completed Successfully</h2>
                                                <p>Dear User,</p>
                                                <p>The full backup has been created and is available for your reference.</p>
                                                <table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
                                                    <tr>
                                                        <td style='padding: 8px; border: 1px solid #ccc;'><strong>File Name:</strong></td>
                                                        <td style='padding: 8px; border: 1px solid #ccc;'>{fileName}</td>
                                                    </tr>
                                                    <tr>
                                                        <td style='padding: 8px; border: 1px solid #ccc;'><strong>File Path:</strong></td>
                                                        <td style='padding: 8px; border: 1px solid #ccc;'>{finalZipPath}</td>
                                                    </tr>
                                                    <tr>
                                                        <td style='padding: 8px; border: 1px solid #ccc;'><strong>Password:</strong></td>
                                                        <td style='padding: 8px; border: 1px solid #ccc;'>{zipPassword}</td>
                                                    </tr>
                                                    <tr>
                                                        <td style='padding: 8px; border: 1px solid #ccc;'><strong>Backup Date:</strong></td>
                                                        <td style='padding: 8px; border: 1px solid #ccc;'>{backupDate}</td>
                                                    </tr>
                                                </table>
                                                <p style='margin-top:20px;'>Please save this file securely. Contact IT support if you face any issues.</p>
                                                <p>Best Regards,<br/>IT Backup System</p>
                                            </body>
                                            </html>";

                            emailSent = await Config.sendEmailAsync(toEmail, subject, body, finalZipPath);
                        }

                    }

                    if (emailSent)
                    {
                        Log.Information("Backup email sent successfully.");
                    }
                    else
                    {
                        Log.Warning("Backup email sent failed.");
                    }

                    // Upload to FTP if enabled
                    if (FTP)
                    {
                        bool success = await Config.UploadToFtpAsync(
                            finalZipPath,
                            1, // organizationId
                            ftpUrl,
                            ftpUsername,
                            ftpPassword
                        );

                        if (success)
                        {
                            Log.Information("File uploaded to FTP successfully.");
                        }
                        else
                        {
                            Log.Error("FTP upload failed.");
                        }
                    }
                    else
                    {
                        Log.Information("FTP upload skipped because FTP option is disabled.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Backup process failed: {ex.Message}");
                }

                return new BackupResult
                {
                    Success = true,
                    BackupPath = finalZipPath,
                    Message = "Password-protected full backup completed successfully",
                    Timestamp = timestamp,
                    Details = new
                    {
                        DatabaseBackup = dbResult.BackupPath,
                        ImagesBackup = imagesResult.BackupPath,
                        FileSize = new FileInfo(finalZipPath).Length
                    }
                };
            }
            else
            {
                var errors = results.Where(r => !r.Success).Select(r => r.Message);
                Log.Warning("Partial backup completed with errors: {Errors}", string.Join(", ", errors));

                // Clean up the backup folder if partial failure
                if (Directory.Exists(backupFolder))
                {
                    Directory.Delete(backupFolder, true);
                }

                return new BackupResult
                {
                    Success = false,
                    BackupPath = null,
                    Error = string.Join("; ", errors),
                    Message = "Backup completed with some errors",
                    Timestamp = timestamp,
                    Details = new
                    {
                        DatabaseBackup = dbResult.Success ? dbResult.BackupPath : null,
                        ImagesBackup = imagesResult.Success ? imagesResult.BackupPath : null,
                        Errors = errors
                    }
                };
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during full backup process");
            return new BackupResult
            {
                Success = false,
                Error = ex.Message,
                Message = "Full backup failed"
            };
        }
    }

    // Method to create password-protected zip file using DotNetZip (unchanged)
    private async Task CreatePasswordProtectedZip(string sourceFolder, string zipPath, string password)
    {
        await Task.Run(() =>
        {
            using (var zip = new Ionic.Zip.ZipFile())
            {
                zip.Password = password;
                zip.Encryption = Ionic.Zip.EncryptionAlgorithm.WinZipAes256;
                //zip.CompressionLevel = Ionic.Zip.CompressionLevel.BestCompression;
                zip.AddDirectory(sourceFolder);
                zip.Save(zipPath);
            }
        });
    }

    // Helper method to backup database to a specific folder (updated for SQL Server)
    private async Task<BackupResult> BackupSqlServerDatabaseToFolderAsync(string targetFolder, string timestamp)
    {
        try
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(DBConnection);
            var databaseName = connectionStringBuilder.InitialCatalog;
            var serverName = connectionStringBuilder.DataSource;
            
            // SIMPLIFIED APPROACH: Use the configured backup path directly
            // SQL Server can only write to directories that already exist
            
            // Normalize the target folder path (remove trailing slashes)
            var backupDirectory = targetFolder.TrimEnd('\\', '/');
            
            // Check if SQL Server is local or remote
            bool isLocalServer = string.IsNullOrEmpty(serverName) || 
                               serverName.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                               serverName.Equals("(local)", StringComparison.OrdinalIgnoreCase) ||
                               serverName.Equals(".", StringComparison.OrdinalIgnoreCase) ||
                               (!serverName.Contains("\\") && !serverName.Contains("."));
            
            // For local SQL Server, ensure directory exists
            if (isLocalServer)
            {
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                    Log.Information("Created backup directory: {BackupDir}", backupDirectory);
                }
            }
            else
            {
                // Remote SQL Server - the directory must exist on SQL Server machine
                // For remote servers, use UNC path (\\server\share\path) or path on SQL Server
                if (!backupDirectory.StartsWith("\\\\"))
                {
                    Log.Warning("Remote SQL Server detected. Ensure backup path '{BackupPath}' exists on SQL Server machine. For best results, use a UNC path (\\\\server\\share\\path).", backupDirectory);
                }
            }
            
            // Connect to master database to perform backup
            var masterConnectionString = new SqlConnectionStringBuilder(DBConnection)
            {
                InitialCatalog = "master"
            };

            using var connection = new SqlConnection(masterConnectionString.ConnectionString);
            await connection.OpenAsync();

            // Create simple backup filename (no nested paths)
            var backupFileName = $"{databaseName}_{timestamp}.bak";
            var backupFile = Path.Combine(backupDirectory, backupFileName);
            
            Log.Information("Backing up database {Database} to {BackupFile}", databaseName, backupFile);

            // Escape the backup file path for SQL (replace single quotes with double single quotes)
            var escapedBackupFile = backupFile.Replace("'", "''");
            
            // Build the backup command
            var backupCommand = $@"
                BACKUP DATABASE [{databaseName}] 
                TO DISK = '{escapedBackupFile}' 
                WITH FORMAT, 
                     MEDIANAME = 'SQLServerBackup', 
                     NAME = 'Full Backup of {databaseName}',
                     STATS = 10";

            using var command = new SqlCommand(backupCommand, connection);
            command.CommandTimeout = 3600; // 1 hour timeout for large databases

            Log.Information("Starting SQL Server backup: {Database} to {BackupFile}", databaseName, backupFile);
            
            await command.ExecuteNonQueryAsync();

            // For remote servers, the file might be on SQL Server machine, so we can't verify with File.Exists
            // Instead, verify by querying SQL Server
            using var verifyCommand = new SqlCommand($@"
                SELECT physical_device_name, backup_size, backup_finish_date 
                FROM msdb.dbo.backupset bs
                INNER JOIN msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
                WHERE database_name = '{databaseName.Replace("'", "''")}'
                AND backup_finish_date > DATEADD(minute, -5, GETDATE())
                ORDER BY backup_finish_date DESC", connection);
            
            using var reader = await verifyCommand.ExecuteReaderAsync();
            if (reader.HasRows && await reader.ReadAsync())
            {
                var actualBackupPath = reader.GetString(0);
                var backupSize = reader.GetInt64(1);
                Log.Information("SQL Server database backup verified: {BackupFile}, Size: {Size} bytes", actualBackupPath, backupSize);
                
                // Update backupFile to the actual path returned by SQL Server
                backupFile = actualBackupPath;
                
                return new BackupResult
                {
                    Success = true,
                    BackupPath = backupFile,
                    Message = "Database backup completed successfully",
                    Timestamp = timestamp
                };
            }
            else
            {
                // If we can't verify via SQL, check if file exists (for local backups)
                // Check if server is local by checking if path exists
                bool isLocal = File.Exists(backupFile) || Directory.Exists(backupDirectory);
                if (isLocal && File.Exists(backupFile))
                {
                    var fileInfo = new FileInfo(backupFile);
                    Log.Information("SQL Server database backup created successfully: {BackupFile}, Size: {Size} bytes", backupFile, fileInfo.Length);
                    return new BackupResult
                    {
                        Success = true,
                        BackupPath = backupFile,
                        Message = "Database backup completed successfully",
                        Timestamp = timestamp
                    };
                }
                else
                {
                    Log.Warning("Could not verify backup file creation. Backup may have succeeded but file verification failed.");
                    // Assume success if command executed without exception
                    return new BackupResult
                    {
                        Success = true,
                        BackupPath = backupFile,
                        Message = "Database backup completed (verification inconclusive)",
                        Timestamp = timestamp
                    };
                }
            }
        }
        catch (SqlException sqlEx)
        {
            var errorDetails = $"SQL Server Error {sqlEx.Number}: {sqlEx.Message}";
            if (sqlEx.Errors.Count > 0)
            {
                var sqlErrors = string.Join("; ", sqlEx.Errors.Cast<Microsoft.Data.SqlClient.SqlError>().Select(e => $"{e.Number}: {e.Message}"));
                errorDetails = $"SQL Server Errors: {sqlErrors}";
            }
            
            Log.Error(sqlEx, "SQL Server backup failed: {ErrorDetails}", errorDetails);
            return new BackupResult
            {
                Success = false,
                Error = errorDetails,
                Message = $"Database backup failed: {sqlEx.Message}"
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating SQL Server database backup: {ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
            return new BackupResult
            {
                Success = false,
                Error = $"{ex.GetType().Name}: {ex.Message}",
                Message = "Database backup failed"
            };
        }
    }

    // Helper method to backup images to a specific folder (unchanged)
    private async Task<BackupResult> BackupImagesFolderToFolderAsync(string targetFolder, string timestamp)
    {
        try
        {
            var imagesFolder = await SystemConfigurationHelper.GetConfigValueAsync("Images", "FolderPath") ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(imagesFolder))
            {
                Log.Warning("Images folder not found: {ImagesFolder}", imagesFolder);
                return new BackupResult
                {
                    Success = false,
                    Error = "Images folder not found",
                    Message = "Images backup failed - folder not found"
                };
            }

            var imagesBackupFile = Path.Combine(targetFolder, $"images_{timestamp}.backup");

            // Create zip of images folder using System.IO.Compression
            await Task.Run(() => System.IO.Compression.ZipFile.CreateFromDirectory(imagesFolder, imagesBackupFile, CompressionLevel.Optimal, false));

            Log.Information("Images folder backup created: {BackupFile}", imagesBackupFile);

            return new BackupResult
            {
                Success = true,
                BackupPath = imagesBackupFile,
                Message = "Images backup completed successfully",
                Timestamp = timestamp
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error backing up images folder");
            return new BackupResult
            {
                Success = false,
                Error = ex.Message,
                Message = "Images backup failed"
            };
        }
    }
}
