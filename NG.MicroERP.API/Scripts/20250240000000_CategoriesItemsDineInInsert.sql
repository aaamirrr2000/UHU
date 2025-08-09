-- Enable explicit ID insertion for Categories
SET IDENTITY_INSERT Categories ON;

INSERT INTO [dbo].[Categories] (
    [Id], [OrganizationId], [Code], [Pic], [Name], [ParentId], 
    [IsActive], [CreatedBy], [CreatedOn], [CreatedFrom], 
    [UpdatedBy], [UpdatedOn], [UpdatedFrom], [IsSoftDeleted]
) 
VALUES 
(1, 1, 'CAT000001', 'files/CAT-001.jpg', 'ACCESSORIES', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(2, 1, 'CAT000002', 'files/CAT-001.jpg', 'AMP', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(3, 1, 'CAT000003', 'files/CAT-001.jpg', 'ASSETS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(4, 1, 'CAT000004', 'files/CAT-001.jpg', 'BAG', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(5, 1, 'CAT000005', 'files/CAT-001.jpg', 'CABLE', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(6, 1, 'CAT000006', 'files/CAT-001.jpg', 'CAJONS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(7, 1, 'CAT000007', 'files/CAT-001.jpg', 'CAPO', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(8, 1, 'CAT000008', 'files/CAT-001.jpg', 'DEJEMBES', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(9, 1, 'CAT000009', 'files/CAT-001.jpg', 'GUITARS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(10, 1, 'CAT000010', 'files/CAT-001.jpg', 'HOLDER', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(11, 1, 'CAT000011', 'files/CAT-001.jpg', 'MENDOLINS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(12, 1, 'CAT000012', 'files/CAT-001.jpg', 'PARTS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(13, 1, 'CAT000013', 'files/CAT-001.jpg', 'RABABS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(14, 1, 'CAT000014', 'files/CAT-001.jpg', 'STAND', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(15, 1, 'CAT000015', 'files/CAT-001.jpg', 'STRAP', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(16, 1, 'CAT000016', 'files/CAT-001.jpg', 'STRING', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(17, 1, 'CAT000017', 'files/CAT-001.jpg', 'UKULELES', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(18, 1, 'CAT000018', 'files/CAT-001.jpg', 'VIOLINS', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(19, 1, 'CAT000019', 'files/CAT-001.jpg', 'INSTRUMENT', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(20, 1, 'CAT000020', 'files/CAT-001.jpg', 'RECORDER', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(21, 1, 'CAT000021', 'files/CAT-001.jpg', 'SERVICE', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(22, 1, 'CAT000022', 'files/CAT-001.jpg', 'KEYBOARD', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(23, 1, 'CAT000022', 'files/CAT-001.jpg', 'DRUM', 0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0);


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
(1, 1, 'https://localhost:7019/files/guitar1.jpg', 'GTR001', 'Yamaha F310', 'Acoustic guitar for beginners', 1.00, 5.00, 0.000000, 5.000000, 12000.00, 14500.00, 9, 'Piece', 'Unit', '[{"Size":"Standard","Price":14500,"Pic":"https://localhost:7019/files/guitar1.jpg"},{"Size":"With Bag","Price":15500,"Pic":"https://localhost:7019/files/guitar1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(2, 1, 'https://localhost:7019/files/guitar2.jpg', 'GTR002', 'Fender Stratocaster', 'Electric guitar with classic tone', 1.00, 3.00, 5.000000, 10.000000, 60000.00, 72000.00, 9, 'Piece', 'Unit', '[{"Size":"Standard","Price":72000,"Pic":"https://localhost:7019/files/guitar2.jpg"},{"Size":"With Amp","Price":80000,"Pic":"https://localhost:7019/files/guitar2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(3, 1, 'https://localhost:7019/files/keyboard1.jpg', 'KYB001', 'Casio CTK-3500', '61-key portable keyboard', 1.00, 4.00, 0.000000, 5.000000, 18000.00, 22000.00, 22, 'Piece', 'Unit', '[{"Size":"Standard","Price":22000,"Pic":"https://localhost:7019/files/keyboard1.jpg"},{"Size":"With Stand","Price":25000,"Pic":"https://localhost:7019/files/keyboard1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(4, 1, 'https://localhost:7019/files/keyboard2.jpg', 'KYB002', 'Yamaha PSR-E373', 'Touch-sensitive keyboard with 622 voices', 1.00, 2.00, 0.000000, 5.000000, 32000.00, 38000.00, 22, 'Piece', 'Unit', '[{"Size":"Standard","Price":38000,"Pic":"https://localhost:7019/files/keyboard2.jpg"},{"Size":"With Adapter","Price":40000,"Pic":"https://localhost:7019/files/keyboard2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(5, 1, 'https://localhost:7019/files/drum1.jpg', 'DRM001', 'Roland V-Drums TD-1K', 'Electronic drum kit for practice', 1.00, 2.00, 0.000000, 8.000000, 85000.00, 95000.00, 23, 'Set', 'Unit', '[{"Size":"5-Piece","Price":95000,"Pic":"https://localhost:7019/files/drum1.jpg"},{"Size":"With Stool","Price":99000,"Pic":"https://localhost:7019/files/drum1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(6, 1, 'https://localhost:7019/files/drum2.jpg', 'DRM002', 'Yamaha Stage Custom', 'Acoustic drum set with birch shells', 1.00, 1.00, 0.000000, 10.000000, 95000.00, 110000.00, 23, 'Set', 'Unit', '[{"Size":"Standard","Price":110000,"Pic":"https://localhost:7019/files/drum2.jpg"},{"Size":"With Cymbals","Price":120000,"Pic":"https://localhost:7019/files/drum2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(7, 1, 'https://localhost:7019/files/violin1.jpg', 'VLN001', 'Stentor Student II', 'Full-size beginner violin', 1.00, 5.00, 0.000000, 5.000000, 9000.00, 11000.00, 18, 'Piece', 'Unit', '[{"Size":"4/4","Price":11000,"Pic":"https://localhost:7019/files/violin1.jpg"},{"Size":"3/4","Price":9500,"Pic":"https://localhost:7019/files/violin1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(8, 1, 'https://localhost:7019/files/violin2.jpg', 'VLN002', 'Cremona SV-500', 'Professional model with ebony fittings', 1.00, 3.00, 0.000000, 8.000000, 22000.00, 26000.00, 19, 'Piece', 'Unit', '[{"Size":"4/4","Price":26000,"Pic":"https://localhost:7019/files/violin2.jpg"},{"Size":"With Case","Price":28000,"Pic":"https://localhost:7019/files/violin2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(9, 1, 'https://localhost:7019/files/amp1.jpg', 'AMP001', 'Marshall MG15', '15-watt practice amp with overdrive', 1.00, 5.00, 0.000000, 5.000000, 10000.00, 12500.00, 2, 'Piece', 'Unit', '[{"Size":"Standard","Price":12500,"Pic":"https://localhost:7019/files/amp1.jpg"},{"Size":"With Cables","Price":13500,"Pic":"https://localhost:7019/files/amp1b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(10, 1, 'https://localhost:7019/files/amp2.jpg', 'AMP002', 'Fender Frontman 10G', '10-watt electric guitar amp', 1.00, 4.00, 0.000000, 5.000000, 8500.00, 10000.00, 2, 'Piece', 'Unit', '[{"Size":"Standard","Price":10000,"Pic":"https://localhost:7019/files/amp2.jpg"},{"Size":"With Cable","Price":11000,"Pic":"https://localhost:7019/files/amp2b.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(11, 1, 'https://localhost:7019/files/stand1.jpg', 'STD001', 'Adjustable Music Stand', 'Foldable sheet music stand', 1.00, 10.00, 0.000000, 6.000000, 2000.00, 2500.00, 14, 'Piece', 'Unit', '[{"Size":"Standard","Price":2500,"Pic":"https://localhost:7019/files/stand1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(12, 1, 'https://localhost:7019/files/strap1.jpg', 'STR001', 'Leather Guitar Strap', 'Comfortable leather strap for guitar', 1.00, 15.00, 0.000000, 7.000000, 1200.00, 1600.00, 15, 'Piece', 'Unit', '[{"Size":"Standard","Price":1600,"Pic":"https://localhost:7019/files/strap1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(13, 1, 'https://localhost:7019/files/string1.jpg', 'STRG001', 'D’Addario EJ16', 'Phosphor Bronze Acoustic Guitar Strings', 1.00, 30.00, 0.000000, 8.000000, 600.00, 850.00, 16, 'Pack', 'Unit', '[{"Size":"Set","Price":850,"Pic":"https://localhost:7019/files/string1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(14, 1, 'https://localhost:7019/files/ukulele1.jpg', 'UKL001', 'Kala KA-15S', 'Soprano ukulele with mahogany body', 1.00, 4.00, 0.000000, 5.000000, 9000.00, 10500.00, 17, 'Piece', 'Unit', '[{"Size":"Standard","Price":10500,"Pic":"https://localhost:7019/files/ukulele1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(15, 1, 'https://localhost:7019/files/violin3.jpg', 'VLN003', 'Yamaha V5SC', 'Intermediate violin with case and bow', 1.00, 2.00, 0.000000, 5.000000, 30000.00, 36000.00, 18, 'Piece', 'Unit', '[{"Size":"4/4","Price":36000,"Pic":"https://localhost:7019/files/violin3.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(16, 1, 'https://localhost:7019/files/instrument1.jpg', 'INS001', 'Flute - Beginner Model', 'Closed hole silver-plated flute', 1.00, 5.00, 0.000000, 10.000000, 7000.00, 8800.00, 19, 'Piece', 'Unit', '[{"Size":"Standard","Price":8800,"Pic":"https://localhost:7019/files/instrument1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(17, 1, 'https://localhost:7019/files/recorder1.jpg', 'REC001', 'Yamaha YRS-23', 'Soprano Recorder with Baroque Fingering', 1.00, 10.00, 0.000000, 11.000000, 800.00, 1000.00, 20, 'Piece', 'Unit', '[{"Size":"Standard","Price":1000,"Pic":"https://localhost:7019/files/recorder1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(18, 1, 'https://localhost:7019/files/capo1.jpg', 'CAP001', 'Kyser Quick-Change Capo', 'Capo for 6-string acoustic guitars', 1.00, 10.00, 0.000000, 12.000000, 1200.00, 1500.00, 7, 'Piece', 'Unit', '[{"Size":"Standard","Price":1500,"Pic":"https://localhost:7019/files/capo1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(19, 1, 'https://localhost:7019/files/bag1.jpg', 'BAG001', 'Padded Guitar Bag', 'Protective carrying bag for guitar', 1.00, 10.00, 0.000000, 13.000000, 2500.00, 3000.00, 4, 'Piece', 'Unit', '[{"Size":"Standard","Price":3000,"Pic":"https://localhost:7019/files/bag1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0),
(20, 1, 'https://localhost:7019/files/service1.jpg', 'SVC001', 'Guitar Tuning Service', 'Professional guitar tuning by expert', 1.00, 5.00, 0.000000, 14.000000, 1000.00, 1500.00, 21, 'Service', 'Unit', '[{"Size":"Per Session","Price":1500,"Pic":"https://localhost:7019/files/service1.jpg"}]', 1, 0, 1, 1, '2025-08-01 12:36:28.033', 'localhost', 1, '2025-08-01 12:36:28.033', 'localhost', 0);

SET IDENTITY_INSERT Items OFF;
GO
