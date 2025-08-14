-- Enable explicit ID insertion for Categories
SET IDENTITY_INSERT Categories ON;

INSERT INTO [dbo].[Categories] (
    [Id], [OrganizationId], [Code], [Pic], [Name], [ParentId], 
    [IsActive], [CreatedBy], [CreatedOn], [CreatedFrom], 
    [UpdatedBy], [UpdatedOn], [UpdatedFrom], [IsSoftDeleted]
) 
VALUES 
(1, 1, 'CAT000001', null, 'ACCESSORIES', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(2, 1, 'CAT000002', null, 'AMP', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(3, 1, 'CAT000003', null, 'ASSETS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(4, 1, 'CAT000004', null, 'BAG', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(5, 1, 'CAT000005', null, 'CABLE', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(6, 1, 'CAT000006', null, 'CAJONS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(7, 1, 'CAT000007', null, 'CAPO', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(8, 1, 'CAT000008', null, 'DEJEMBES', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(9, 1, 'CAT000009', null, 'GUITARS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(10, 1, 'CAT000010', null, 'HOLDER', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(11, 1, 'CAT000011', null, 'MENDOLINS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(12, 1, 'CAT000012', null, 'PARTS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(13, 1, 'CAT000013', null, 'RABABS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(14, 1, 'CAT000014', null, 'STAND', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(15, 1, 'CAT000015', null, 'STRAP', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(16, 1, 'CAT000016', null, 'STRING', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(17, 1, 'CAT000017', null, 'UKULELES', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(18, 1, 'CAT000018', null, 'VIOLINS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(19, 1, 'CAT000019', null, 'INSTRUMENT', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(20, 1, 'CAT000020', null, 'RECORDER', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(21, 1, 'CAT000021', null, 'SERVICE', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(22, 1, 'CAT000022', null, 'KEYBOARD', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(23, 1, 'CAT000022', null, 'DRUM', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0);


SET IDENTITY_INSERT Categories OFF;
GO

-- Enable explicit ID insertion for Items
SET IDENTITY_INSERT Items ON;


INSERT INTO Items (
    [Id], [OrganizationId], [Pic], [Code], [Name], [Description], [MinQty], [MaxQty], [Discount], [Tax], [CostPrice], [RetailPrice], 
    [CategoriesId], [StockType], [Unit], [ServingSize], [IsInventoryItem], [IsFavItem], [IsActive], [CreatedBy], [CreatedOn], [CreatedFrom], 
    [UpdatedBy], [UpdatedOn], [UpdatedFrom], [IsSoftDeleted]
)
VALUES
(1, 1, null, 'GTR001', 'Yamaha F310', 'Acoustic guitar for beginners', 1.00, 5.00, 0.000000, 5.000000, 12000.00, 14500.00, 9, 'Piece', 'Unit', '[{"Size":"Standard","Price":14500,"Pic":"https://localhost:7019/files/guitar1.jpg"},{"Size":"With Bag","Price":15500,"Pic":"https://localhost:7019/files/guitar1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(2, 1, null, 'GTR002', 'Fender Stratocaster', 'Electric guitar with classic tone', 1.00, 3.00, 5.000000, 10.000000, 60000.00, 72000.00, 9, 'Piece', 'Unit', '[{"Size":"Standard","Price":72000,"Pic":"https://localhost:7019/files/guitar2.jpg"},{"Size":"With Amp","Price":80000,"Pic":"https://localhost:7019/files/guitar2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(3, 1, null, 'KYB001', 'Casio CTK-3500', '61-key portable keyboard', 1.00, 4.00, 0.000000, 5.000000, 18000.00, 22000.00, 22, 'Piece', 'Unit', '[{"Size":"Standard","Price":22000,"Pic":"https://localhost:7019/files/keyboard1.jpg"},{"Size":"With Stand","Price":25000,"Pic":"https://localhost:7019/files/keyboard1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(4, 1, null, 'KYB002', 'Yamaha PSR-E373', 'Touch-sensitive keyboard with 622 voices', 1.00, 2.00, 0.000000, 5.000000, 32000.00, 38000.00, 22, 'Piece', 'Unit', '[{"Size":"Standard","Price":38000,"Pic":"https://localhost:7019/files/keyboard2.jpg"},{"Size":"With Adapter","Price":40000,"Pic":"https://localhost:7019/files/keyboard2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(5, 1, null, 'DRM001', 'Roland V-Drums TD-1K', 'Electronic drum kit for practice', 1.00, 2.00, 0.000000, 8.000000, 85000.00, 95000.00, 23, 'Set', 'Unit', '[{"Size":"5-Piece","Price":95000,"Pic":"https://localhost:7019/files/drum1.jpg"},{"Size":"With Stool","Price":99000,"Pic":"https://localhost:7019/files/drum1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(6, 1, null, 'DRM002', 'Yamaha Stage Custom', 'Acoustic drum set with birch shells', 1.00, 1.00, 0.000000, 10.000000, 95000.00, 110000.00, 23, 'Set', 'Unit', '[{"Size":"Standard","Price":110000,"Pic":"https://localhost:7019/files/drum2.jpg"},{"Size":"With Cymbals","Price":120000,"Pic":"https://localhost:7019/files/drum2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(7, 1, null, 'VLN001', 'Stentor Student II', 'Full-size beginner violin', 1.00, 5.00, 0.000000, 5.000000, 9000.00, 11000.00, 18, 'Piece', 'Unit', '[{"Size":"4/4","Price":11000,"Pic":"https://localhost:7019/files/violin1.jpg"},{"Size":"3/4","Price":9500,"Pic":"https://localhost:7019/files/violin1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(8, 1, null, 'VLN002', 'Cremona SV-500', 'Professional model with ebony fittings', 1.00, 3.00, 0.000000, 8.000000, 22000.00, 26000.00, 19, 'Piece', 'Unit', '[{"Size":"4/4","Price":26000,"Pic":"https://localhost:7019/files/violin2.jpg"},{"Size":"With Case","Price":28000,"Pic":"https://localhost:7019/files/violin2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(9, 1, null, 'AMP001', 'Marshall MG15', '15-watt practice amp with overdrive', 1.00, 5.00, 0.000000, 5.000000, 10000.00, 12500.00, 2, 'Piece', 'Unit', '[{"Size":"Standard","Price":12500,"Pic":"https://localhost:7019/files/amp1.jpg"},{"Size":"With Cables","Price":13500,"Pic":"https://localhost:7019/files/amp1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(10, 1, null, 'AMP002', 'Fender Frontman 10G', '10-watt electric guitar amp', 1.00, 4.00, 0.000000, 5.000000, 8500.00, 10000.00, 2, 'Piece', 'Unit', '[{"Size":"Standard","Price":10000,"Pic":"https://localhost:7019/files/amp2.jpg"},{"Size":"With Cable","Price":11000,"Pic":"https://localhost:7019/files/amp2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(11, 1, null, 'STD001', 'Adjustable Music Stand', 'Foldable sheet music stand', 1.00, 10.00, 0.000000, 6.000000, 2000.00, 2500.00, 14, 'Piece', 'Unit', '[{"Size":"Standard","Price":2500,"Pic":"https://localhost:7019/files/stand1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(12, 1, null, 'STR001', 'Leather Guitar Strap', 'Comfortable leather strap for guitar', 1.00, 15.00, 0.000000, 7.000000, 1200.00, 1600.00, 15, 'Piece', 'Unit', '[{"Size":"Standard","Price":1600,"Pic":"https://localhost:7019/files/strap1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(13, 1, null, 'STRG001', 'D’Addario EJ16', 'Phosphor Bronze Acoustic Guitar Strings', 1.00, 30.00, 0.000000, 8.000000, 600.00, 850.00, 16, 'Pack', 'Unit', '[{"Size":"Set","Price":850,"Pic":"https://localhost:7019/files/string1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(14, 1, null, 'UKL001', 'Kala KA-15S', 'Soprano ukulele with mahogany body', 1.00, 4.00, 0.000000, 5.000000, 9000.00, 10500.00, 17, 'Piece', 'Unit', '[{"Size":"Standard","Price":10500,"Pic":"https://localhost:7019/files/ukulele1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(15, 1, null, 'VLN003', 'Yamaha V5SC', 'Intermediate violin with case and bow', 1.00, 2.00, 0.000000, 5.000000, 30000.00, 36000.00, 18, 'Piece', 'Unit', '[{"Size":"4/4","Price":36000,"Pic":"https://localhost:7019/files/violin3.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(16, 1, null, 'INS001', 'Flute - Beginner Model', 'Closed hole silver-plated flute', 1.00, 5.00, 0.000000, 10.000000, 7000.00, 8800.00, 19, 'Piece', 'Unit', '[{"Size":"Standard","Price":8800,"Pic":"https://localhost:7019/files/instrument1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(17, 1, null, 'REC001', 'Yamaha YRS-23', 'Soprano Recorder with Baroque Fingering', 1.00, 10.00, 0.000000, 11.000000, 800.00, 1000.00, 20, 'Piece', 'Unit', '[{"Size":"Standard","Price":1000,"Pic":"https://localhost:7019/files/recorder1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(18, 1, null, 'CAP001', 'Kyser Quick-Change Capo', 'Capo for 6-string acoustic guitars', 1.00, 10.00, 0.000000, 12.000000, 1200.00, 1500.00, 7, 'Piece', 'Unit', '[{"Size":"Standard","Price":1500,"Pic":"https://localhost:7019/files/capo1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(19, 1, null, 'BAG001', 'Padded Guitar Bag', 'Protective carrying bag for guitar', 1.00, 10.00, 0.000000, 13.000000, 2500.00, 3000.00, 4, 'Piece', 'Unit', '[{"Size":"Standard","Price":3000,"Pic":"https://localhost:7019/files/bag1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(20, 1, null, 'SVC001', 'Guitar Tuning Service', 'Professional guitar tuning by expert', 1.00, 5.00, 0.000000, 14.000000, 1000.00, 1500.00, 21, 'Service', 'Unit', '[{"Size":"Per Session","Price":1500,"Pic":"https://localhost:7019/files/service1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0);

UPDATE Items SET PCTCode = 'YMH-F310' WHERE Code = 'GTR001';
UPDATE Items SET PCTCode = 'FND-STRAT' WHERE Code = 'GTR002';
UPDATE Items SET PCTCode = 'CS-CTK3500' WHERE Code = 'KYB001';
UPDATE Items SET PCTCode = 'YMH-PSRE373' WHERE Code = 'KYB002';
UPDATE Items SET PCTCode = 'RLD-TD1K' WHERE Code = 'DRM001';
UPDATE Items SET PCTCode = 'YMH-STAGE' WHERE Code = 'DRM002';
UPDATE Items SET PCTCode = 'STNT-STU2' WHERE Code = 'VLN001';
UPDATE Items SET PCTCode = 'CRM-SV500' WHERE Code = 'VLN002';
UPDATE Items SET PCTCode = 'MRSH-MG15' WHERE Code = 'AMP001';
UPDATE Items SET PCTCode = 'FND-FRNT10G' WHERE Code = 'AMP002';
UPDATE Items SET PCTCode = 'GEN-MUSSTD' WHERE Code = 'STD001';
UPDATE Items SET PCTCode = 'GEN-GTRSTRP' WHERE Code = 'STR001';
UPDATE Items SET PCTCode = 'DAD-EJ16' WHERE Code = 'STRG001';
UPDATE Items SET PCTCode = 'KALA-KA15S' WHERE Code = 'UKL001';
UPDATE Items SET PCTCode = 'YMH-V5SC' WHERE Code = 'VLN003';
UPDATE Items SET PCTCode = 'GEN-FLUTE' WHERE Code = 'INS001';
UPDATE Items SET PCTCode = 'YMH-YRS23' WHERE Code = 'REC001';
UPDATE Items SET PCTCode = 'KYS-QCCAPO' WHERE Code = 'CAP001';
UPDATE Items SET PCTCode = 'GEN-GTRBAG' WHERE Code = 'BAG001';
UPDATE Items SET PCTCode = 'GEN-TUNINGSVC' WHERE Code = 'SVC001';

SET IDENTITY_INSERT Items OFF;
GO
