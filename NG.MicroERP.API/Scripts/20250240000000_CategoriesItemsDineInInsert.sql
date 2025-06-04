INSERT INTO Categories (
    Guid, OrganizationId, Code, Name, ParentId, IsActive,
    CreatedBy, CreatedOn, CreatedFrom,
    UpdatedBy, UpdatedOn, UpdatedFrom,
    IsSoftDeleted
)
VALUES
(NEWID(), 1, 'CAT-001', 'Starters',     0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(NEWID(), 1, 'CAT-002', 'Main Course',  0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(NEWID(), 1, 'CAT-003', 'Desserts',     0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(NEWID(), 1, 'CAT-004', 'Drinks',       0, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0),
(NEWID(), 1, 'CAT-005', 'Pizza',        2, 1, 1, GETDATE(), 'AdminPC', 1, GETDATE(), 'AdminPC', 0);


INSERT INTO Items (Guid, OrganizationId, Pic, Code, Name, Description, MinQty, MaxQty, Discount, CostPrice, RetailPrice, CategoriesId, StockType, Unit, IsActive, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted, RowVersion, ServingSize)
VALUES 
(NEWID(), 1, 'https://example.com/images/spring_rolls.jpg', 'ST-001', 'Spring Rolls', 'Crispy vegetable spring rolls', 1, 10, 0, 40, 80, 1, 'Food', 'Plate', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, N'[{"Size":"4 pcs", "Price":80}, {"Size":"8 pcs", "Price":150}]'),
(NEWID(), 1, 'https://example.com/images/garlic_bread.jpg', 'ST-002', 'Garlic Bread', 'Toasted bread with garlic and herbs', 1, 10, 0, 30, 60, 1, 'Food', 'Plate', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, N'[{"Size":"Half", "Price":60}, {"Size":"Full", "Price":110}]');

INSERT INTO Items (Guid, OrganizationId, Pic, Code, Name, Description, MinQty, MaxQty, Discount, CostPrice, RetailPrice, CategoriesId, StockType, Unit, IsActive, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted, RowVersion, ServingSize)
VALUES 
(NEWID(), 1, 'https://example.com/images/chicken_biryani.jpg', 'MC-001', 'Chicken Biryani', 'Spicy chicken biryani served with raita', 1, 10, 0, 200, 350, 2, 'Food', 'Plate', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, N'[{"Size":"Small","Price":250},{"Size":"Regular","Price":350},{"Size":"Medium","Price":400},{"Size":"Large","Price":500},{"Size":"Extra Large","Price":600},{"Size":"Jumbo","Price":750},{"Size":"Family Pack","Price":1000}]'),
(NEWID(), 1, 'https://example.com/images/paneer_butter_masala.jpg', 'MC-002', 'Paneer Butter Masala', 'Paneer in rich butter masala gravy', 1, 10, 0, 180, 320, 2, 'Food', 'Bowl', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, N'[{"Size":"Half", "Price":180}, {"Size":"Full", "Price":320}]'),
(NEWID(), 1, 'https://example.com/images/cheese_pizza.jpg', 'MC-003', 'Cheese Pizza', 'Cheesy pizza with herbs', 1, 10, 0, 250, 450, 5, 'Food', 'Piece', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, N'[{"Size":"Small", "Price":299}, {"Size":"Medium", "Price":450}, {"Size":"Large", "Price":599}]');

INSERT INTO Items (Guid, OrganizationId, Pic, Code, Name, Description, MinQty, MaxQty, Discount, CostPrice, RetailPrice, CategoriesId, StockType, Unit, IsActive, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted, RowVersion, ServingSize)
VALUES 
(NEWID(), 1, 'https://example.com/images/gulab_jamun.jpg', 'DS-001', 'Gulab Jamun', 'Soft sweet balls in sugar syrup', 1, 10, 0, 30, 60, 3, 'Food', 'Bowl', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, N'[{"Size":"2 pcs", "Price":60}, {"Size":"4 pcs", "Price":110}]'),
(NEWID(), 1, 'https://example.com/images/ice_cream.jpg', 'DS-002', 'Ice Cream', 'Vanilla, Chocolate, Strawberry options available', 1, 10, 0, 40, 70, 3, 'Food', 'Cup', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, N'[{"Size":"Single Scoop", "Price":70}, {"Size":"Double Scoop", "Price":120}]');

INSERT INTO Items (Guid, OrganizationId, Pic, Code, Name, Description, MinQty, MaxQty, Discount, CostPrice, RetailPrice, CategoriesId, StockType, Unit, IsActive, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted, RowVersion, ServingSize)
VALUES 
(NEWID(), 1, 'https://example.com/images/cola.jpg', 'DR-001', 'Cola', 'Chilled cola drink', 1, 10, 0, 20, 40, 4, 'Drink', 'Glass', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, N'[{"Size":"250 ml", "Price":40}, {"Size":"500 ml", "Price":60}]'),
(NEWID(), 1, 'https://example.com/images/lemonade.jpg', 'DR-002', 'Lemonade', 'Fresh lemon juice with mint', 1, 10, 0, 25, 50, 4, 'Drink', 'Glass', 1, 1, GETDATE(), 'Init', 1, GETDATE(), 'Init', 0, NULL, N'[{"Size":"Regular", "Price":50}, {"Size":"Large", "Price":70}]');
