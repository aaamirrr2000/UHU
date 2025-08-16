CREATE TABLE DigitalInvoiceScenarios
(
    Id                      INT IDENTITY(1,1) PRIMARY KEY,
    ScenarioID              VARCHAR(10),   -- e.g., SN001
    SaleType    NVARCHAR(255)   NOT NULL,      -- Full description of scenario
    BuyerType               NVARCHAR(50)    NULL,          -- Registered / Unregistered / NULL
    TaxContext      NVARCHAR(500)   NULL           -- Purpose or tax context details
);

INSERT INTO DigitalInvoiceScenarios (ScenarioID, SaleType, BuyerType, TaxContext)
VALUES
('SN001', 'Sale of Standard Rate Goods to Registered Buyers', 'Registered', 'B2B supplies; buyer can claim input tax credit'),
('SN002', 'Sale of Standard Rate Goods to Unregistered Buyers', 'Unregistered', 'B2C sales; buyer cannot claim input tax credit'),
('SN003', 'Sale of Steel (Melted & Re-Rolled)', 'Unregistered', 'Sector-specific rules in steel manufacturing'),
('SN004', 'Sale of Steel Scrap by Ship Breakers', 'Unregistered', 'Special tax treatment for ship-breaking'),
('SN005', 'Sales of Reduced-Rate Goods (Eighth Schedule)', 'Unregistered', 'Reduced rate (e.g., 1%) to ensure affordability of essential items'),
('SN006', 'Sale of Exempt Goods (Sixth Schedule)', 'Registered', 'Tax-exempt items like certain agriculture/medicine categories'),
('SN007', 'Sale of Zero-Rated Goods (Fifth Schedule)', 'Unregistered', '0% tax, but seller can still claim input credits—typically exports'),
('SN008', 'Sale of 3rd Schedule Goods', 'Unregistered', 'Items taxed based on printed retail price—not transaction value'),
('SN009', 'Purchase from Registered Cotton Ginners', 'Registered', 'Cotton industry-specific taxation'),
('SN010', 'Sale of Telecom Services by Mobile Operators', 'Unregistered', 'Services (calls, data, SMS) at tax rate (e.g., ~17%)'),
('SN011', 'Sale via Toll Manufacturing (Steel Products)', 'Unregistered', 'Third-party steel processing treatment'),
('SN012', 'Sale of Petroleum Products', 'Unregistered', 'Distinct low rate (e.g., 1.43%) or FEDs for petrol, diesel'),
('SN013', 'Sale of Electricity to Retailers', 'Unregistered', 'Taxed at a specific rate (e.g., around 5%)'),
('SN014', 'Sale of Gas to CNG Stations', 'Unregistered', 'Specialized rate for fuel distribution'),
('SN015', 'Sale of Mobile Phones', 'Unregistered', 'Tax category applied to mobile phone sales'),
('SN016', 'Processing / Conversion of Goods', 'Unregistered', 'Involves value-addition or manufacturing processes'),
('SN017', 'Goods (FED in Sales-Tax Mode)', NULL, 'Items where FED applies in place of or alongside sales tax'),
('SN018', 'Services (FED in Sales-Tax Mode)', NULL, 'Services taxed under federal excise duty regime'),
('SN019', 'Services', NULL, 'Regular services like consulting, rentals, etc.'),
('SN020', 'Electric Vehicles', 'Unregistered', 'EVs taxed under specific scheme'),
('SN021', 'Cement / Concrete Block', 'Unregistered', 'Construction material category'),
('SN022', 'Potassium Chlorate', 'Unregistered', 'Chemical-specific category'),
('SN023', 'CNG Sales', 'Unregistered', 'Special fuel retail tax'),
('SN024', 'Goods as per SRO 297(1)/2023', 'Unregistered', 'Items notified under SRO 297(I)/2023'),
('SN025', 'Non-Adjustable Supplies', 'Unregistered', 'Supplies not eligible for tax adjustment'),
('SN026', 'Goods at Standard Rate (default)', 'Unregistered', 'Duplicate of standard rate, default'),
('SN027', '3rd Schedule Goods', 'Unregistered', 'Duplicate category (like SN008)'),
('SN028', 'Goods at Reduced Rate', 'Unregistered', 'Another reduced-rate category');
