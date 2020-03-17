
CREATE VIEW [svc].[uvw_Claims]
AS
SELECT        Id, IssuerId, ActorId, Subject, Type, Value, ValueType, Created, LastUpdated, Immutable
FROM            dbo.tbl_Claims