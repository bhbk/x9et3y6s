
CREATE VIEW [svc].[uvw_Claim]
AS
SELECT        Id, IssuerId, ActorId, Subject, Type, Value, ValueType, IsDeletable, CreatedUtc, LastUpdatedUtc
FROM            [dbo].[tbl_Claim]