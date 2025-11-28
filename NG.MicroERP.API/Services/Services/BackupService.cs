using Ionic.Zip;
using Microsoft.Data.SqlClient; // Changed from MySql.Data.MySqlClient
using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;

namespace NG.MicroERP.API.Services.Services;

public interface IBackupService
{
    Task<BackupResult> BackupSqlServerDatabaseAsync(); // Renamed from BackupMySqlDatabaseAsync
    Task<BackupResult> BackupImagesFolderAsync();
    Task<BackupResult> BackupAllAsync(bool Email, bool FTP);
}

public class BackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private readonly string _backupBasePath;
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
        _backupBasePath = _configuration["Backup:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups");

        DBConnection = configuration.GetValue<string>("ConnectionStrings:Default") ?? string.Empty;

        if (!DBConnection.Contains("Pooling"))
            DBConnection += ";Pooling=true;Min Pool Size=5;Max Pool Size=50;Connection Lifetime=300;";

        if (!Directory.Exists(_backupBasePath))
        {
            Directory.CreateDirectory(_backupBasePath);
        }
    }

    // Function 1: Backup SQL Server Database using sqlcmd
    public async Task<BackupResult> BackupSqlServerDatabaseAsync() // Renamed method
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFolder = Path.Combine(_backupBasePath, $"DB_Backup_{timestamp}");
            Directory.CreateDirectory(backupFolder);

            var connectionStringBuilder = new SqlConnectionStringBuilder(DBConnection); // Changed to SqlConnectionStringBuilder
            var databaseName = connectionStringBuilder.InitialCatalog;
            var backupFile = Path.Combine(backupFolder, $"{databaseName}_{timestamp}.bak");

            var sqlcmdPath = _configuration["Backup:SqlCmdPath"] ?? "sqlcmd"; // Changed configuration key

            // SQL Server backup using sqlcmd
            var arguments = $"-S {connectionStringBuilder.DataSource} " +
                           $"-d {databaseName} " +
                           $"-U {connectionStringBuilder.UserID} " +
                           $"-P {connectionStringBuilder.Password} " +
                           $"-Q \"BACKUP DATABASE [{databaseName}] TO DISK = '{backupFile}' WITH FORMAT, MEDIANAME = 'SQLServerBackup', NAME = 'Full Backup of {databaseName}'\" " +
                           "-b"; // -b sets sqlcmd to exit when an error occurs

            var processStartInfo = new ProcessStartInfo
            {
                FileName = sqlcmdPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Log.Information("SQL Server database backup created: {BackupFile}", backupFile);
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
                Log.Error("SQL Server backup failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                return new BackupResult
                {
                    Success = false,
                    Error = $"sqlcmd failed: {error}",
                    Message = "Database backup failed"
                };
            }
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
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFolder = Path.Combine(_backupBasePath, $"Images_Backup_{timestamp}");
            Directory.CreateDirectory(backupFolder);

            var imagesFolder = _configuration["Images:FolderPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

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
            // Get values from appsettings.json
            var backupBasePath = _configuration["Backup:BasePath"];
            var baseFilePrefix = _configuration["Backup:BaseFilePrefix"];
            var zipPassword = _configuration["Backup:ZipPassword"];
            var toEmail = _configuration["Backup:BackupEmail"];
            var imagesFolderPath = _configuration["Images:FolderPath"];

            var ftpUrl = _configuration["FTP:ServerUrl"];
            var ftpUsername = _configuration["FTP:Username"];
            var ftpPassword = _configuration["FTP:Password"];


            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFolder = Path.Combine(backupBasePath, $"Full_Backup_{timestamp}");
            Directory.CreateDirectory(backupFolder);

            var results = new List<BackupResult>();

            // Backup database directly into the full backup folder
            var dbResult = await BackupSqlServerDatabaseToFolderAsync(backupFolder, timestamp); // Updated method name
            results.Add(dbResult);

            // Backup images directly into the full backup folder
            var imagesResult = await BackupImagesFolderToFolderAsync(backupFolder, timestamp);
            results.Add(imagesResult);

            // Check if both operations were successful
            var allSuccess = results.All(r => r.Success);

            if (allSuccess)
            {
                // Create final password-protected zip file containing both backups
                var finalZipPath = Path.Combine(backupBasePath, $"{baseFilePrefix}_Backup_{timestamp}.backup");

                if (string.IsNullOrEmpty(zipPassword))
                {
                    throw new Exception("Backup password is not configured in appsettings.json");
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

                            emailSent = Config.sendEmail(toEmail, subject, body, finalZipPath);
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
                            ftpUrl,
                            ftpUsername,
                            ftpPassword,
                            finalZipPath
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
    private async Task<BackupResult> BackupSqlServerDatabaseToFolderAsync(string targetFolder, string timestamp) // Renamed method
    {
        try
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(DBConnection); // Changed to SqlConnectionStringBuilder
            var databaseName = connectionStringBuilder.InitialCatalog;
            var backupFile = Path.Combine(targetFolder, $"{databaseName}_{timestamp}.bak");

            var sqlcmdPath = _configuration["Backup:SqlCmdPath"] ?? "sqlcmd"; // Changed configuration key

            // SQL Server backup command
            var arguments = $"-S {connectionStringBuilder.DataSource} " +
                           $"-d {databaseName} " +
                           $"-U {connectionStringBuilder.UserID} " +
                           $"-P {connectionStringBuilder.Password} " +
                           $"-Q \"BACKUP DATABASE [{databaseName}] TO DISK = '{backupFile}' WITH FORMAT, MEDIANAME = 'SQLServerBackup', NAME = 'Full Backup of {databaseName}'\" " +
                           "-b";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = sqlcmdPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Log.Information("SQL Server database backup created: {BackupFile}", backupFile);
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
                Log.Error("SQL Server backup failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                return new BackupResult
                {
                    Success = false,
                    Error = $"sqlcmd failed: {error}",
                    Message = "Database backup failed"
                };
            }
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

    // Helper method to backup images to a specific folder (unchanged)
    private async Task<BackupResult> BackupImagesFolderToFolderAsync(string targetFolder, string timestamp)
    {
        try
        {
            var imagesFolder = _configuration["Images:FolderPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

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