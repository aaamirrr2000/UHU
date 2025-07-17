-- Enable explicit ID insertion
SET IDENTITY_INSERT Categories ON;

INSERT INTO Categories (
    Id, OrganizationId, Code, Pic, Name, ParentId, IsActive,
    CreatedBy, CreatedOn, CreatedFrom,
    UpdatedBy, UpdatedOn, UpdatedFrom,
    IsSoftDeleted
)
VALUES
(1, 1, 'CAT-001', 'files/CAT-001.jpg', 'Starters',     0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(2, 1, 'CAT-002', 'files/CAT-002.jpg', 'Main Course',  0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(3, 1, 'CAT-003', 'files/CAT-003.jpg', 'Desserts',     0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(4, 1, 'CAT-004', 'files/CAT-004.jpg', 'Drinks',       0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(5, 1, 'CAT-005', 'files/CAT-005.jpg', 'Pizza',        2, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(6, 1, 'CAT-006', 'files/CAT-006.jpg', 'Services',     0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0);


SET IDENTITY_INSERT Categories OFF;


SET IDENTITY_INSERT Items ON;

INSERT INTO Items (
    Id, OrganizationId, Pic, Code, Name, Description, MinQty, MaxQty, Discount,
    CostPrice, RetailPrice, CategoriesId, StockType, Unit, IsActive,
    CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom,
    IsSoftDeleted, RowVersion, ServingSize, IsInventoryItem
)
VALUES
-- Starters
(1, 1, 'https://images.unsplash.com/photo-1617196038435-bd0e66f0e2f6?w=800', 'ST-001', 'Spring Rolls', 'Crispy vegetable rolls served with sauce', 1, 10, 0, 40, 80, 1, 'Food', 'Plate', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"4 pcs","Price":80,"Pic":"https://localhost:7019/files/st1.jpg"},{"Size":"8 pcs","Price":150,"Pic":"https://localhost:7019/files/st2.jpg"}]', 1),
(2, 1, 'https://images.unsplash.com/photo-1550507994-4e7d96224678?w=800', 'ST-002', 'Garlic Bread', 'Toasted bread with garlic and herbs', 1, 10, 0, 30, 60, 1, 'Food', 'Plate', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Half","Price":60,"Pic":"https://localhost:7019/files/st3.jpg"},{"Size":"Full","Price":110,"Pic":"https://localhost:7019/files/st4.jpg"}]', 1),
(3, 1, 'https://images.unsplash.com/photo-1572441710534-680c04b6cfea?w=800', 'ST-003', 'Chicken Wings', 'Spicy grilled chicken wings', 1, 10, 0, 90, 180, 1, 'Food', 'Plate', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"6 pcs","Price":180,"Pic":"https://localhost:7019/files/st5.jpg"}]', 1),
(4, 1, 'https://images.unsplash.com/photo-1628595358103-97b8a3e8c99d?w=800', 'ST-004', 'Fried Wontons', 'Golden crispy wontons with dip', 1, 10, 0, 70, 120, 1, 'Food', 'Plate', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"5 pcs","Price":120,"Pic":"https://localhost:7019/files/st6.jpg"}]', 1),

-- Main Course
(5, 1, 'https://images.unsplash.com/photo-1576402187878-974f87a7d09e?w=800', 'MC-001', 'Chicken Biryani', 'Spicy chicken biryani served with raita', 1, 10, 0, 200, 350, 2, 'Food', 'Plate', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Small","Price":250,"Pic":"https://localhost:7019/files/mc1.jpg"},{"Size":"Regular","Price":350,"Pic":"https://localhost:7019/files/mc2.jpg"},{"Size":"Large","Price":500,"Pic":"https://localhost:7019/files/mc3.jpg"}]', 1),
(6, 1, 'https://images.unsplash.com/photo-1621996346565-c16d9d3c16cc?w=800', 'MC-002', 'Paneer Butter Masala', 'Paneer cubes in creamy tomato gravy', 1, 10, 0, 180, 320, 2, 'Food', 'Bowl', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Half","Price":180,"Pic":"https://localhost:7019/files/mc4.jpg"},{"Size":"Full","Price":320,"Pic":"https://localhost:7019/files/mc5.jpg"}]', 1),
(7, 1, 'https://images.unsplash.com/photo-1594007654729-bef1f6f1cc42?w=800', 'MC-003', 'Beef Steak', 'Grilled steak with pepper sauce', 1, 10, 0, 350, 600, 2, 'Food', 'Plate', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Medium","Price":600,"Pic":"https://localhost:7019/files/mc6.jpg"}]', 1),
(8, 1, 'https://images.unsplash.com/photo-1641579304364-6c6aecc1f0cd?w=800', 'MC-004', 'Butter Chicken', 'Chicken in creamy spiced tomato sauce', 1, 10, 0, 200, 380, 2, 'Food', 'Bowl', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Half","Price":200,"Pic":"https://localhost:7019/files/mc7.jpg"},{"Size":"Full","Price":380,"Pic":"https://localhost:7019/files/mc8.jpg"}]', 1),

-- Desserts
(9, 1, 'https://images.unsplash.com/photo-1601979031925-027d1a9793f4?w=800', 'DS-001', 'Gulab Jamun', 'Soft sweet balls in sugar syrup', 1, 10, 0, 30, 60, 3, 'Food', 'Bowl', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"2 pcs","Price":60,"Pic":"https://localhost:7019/files/ds1.jpg"},{"Size":"4 pcs","Price":110,"Pic":"https://localhost:7019/files/ds2.jpg"}]', 1),
(10, 1, 'https://images.unsplash.com/photo-1542435503-956c469947f6?w=800', 'DS-002', 'Ice Cream', 'Vanilla, Chocolate, Strawberry options available', 1, 10, 0, 40, 70, 3, 'Food', 'Cup', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Single Scoop","Price":70,"Pic":"https://localhost:7019/files/ds3.jpg"},{"Size":"Double Scoop","Price":120,"Pic":"https://localhost:7019/files/ds4.jpg"}]', 1),
(11, 1, 'https://images.unsplash.com/photo-1609250291995-cfd2f64461b5?w=800', 'DS-003', 'Chocolate Brownie', 'Warm brownie with ice cream scoop', 1, 10, 0, 80, 150, 3, 'Food', 'Piece', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Single","Price":150,"Pic":"https://localhost:7019/files/ds5.jpg"}]', 1),
(12, 1, 'https://images.unsplash.com/photo-1621072159433-1cba42cda48b?w=800', 'DS-004', 'Ras Malai', 'Soft paneer in creamy milk sauce', 1, 10, 0, 60, 120, 3, 'Food', 'Bowl', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"2 pcs","Price":120,"Pic":"https://localhost:7019/files/ds6.jpg"}]', 1),

-- Drinks
(13, 1, 'https://images.unsplash.com/photo-1615484477597-df91872b8f1f?w=800', 'DR-001', 'Cola', 'Chilled fizzy cola drink', 1, 10, 0, 20, 40, 4, 'Drink', 'Glass', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"250 ml","Price":40,"Pic":"https://localhost:7019/files/dr1.jpg"},{"Size":"500 ml","Price":60,"Pic":"https://localhost:7019/files/dr2.jpg"}]', 1),
(14, 1, 'https://images.unsplash.com/photo-1580535722401-ec55d5880652?w=800', 'DR-002', 'Lemonade', 'Fresh lemon juice with mint', 1, 10, 0, 25, 50, 4, 'Drink', 'Glass', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Regular","Price":50,"Pic":"https://localhost:7019/files/dr3.jpg"},{"Size":"Large","Price":70,"Pic":"https://localhost:7019/files/dr4.jpg"}]', 1),

-- Services (IsInventoryItem = 0)
(15, 1, 'https://images.unsplash.com/photo-1556741533-f6acd647d2fb?w=800', 'SV-001', 'Courier Service', 'Fast and secure parcel delivery service', 1, 1, 0, 0, 150, 6, 'Service', 'Job', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Standard","Price":150,"Pic":"https://localhost:7019/files/sv1.jpg"},{"Size":"Express","Price":250,"Pic":"https://localhost:7019/files/sv2.jpg"}]', 0),
(16, 1, 'https://images.unsplash.com/photo-1592194996308-7b43878e84a6?w=800', 'SV-002', 'Delivery Service', 'Food and product delivery at your doorstep', 1, 1, 0, 0, 100, 6, 'Service', 'Job', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, 
N'[{"Size":"Within City","Price":100,"Pic":"https://localhost:7019/files/sv3.jpg"},{"Size":"Outskirts","Price":200,"Pic":"https://localhost:7019/files/sv4.jpg"}]', 0);

SET IDENTITY_INSERT Items OFF;

