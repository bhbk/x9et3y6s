
CREATE VIEW [svc].[uvw_Claim]
AS
SELECT        Id, IssuerId, Subject, Type, Value, ValueType, IsDeletable, CreatedUtc, LastUpdatedUtc
FROM            [dbo].[tbl_Claim]