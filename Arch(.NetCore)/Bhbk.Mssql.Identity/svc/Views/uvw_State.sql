
CREATE VIEW [svc].[uvw_State]
AS
SELECT        Id, IssuerId, AudienceId, UserId, StateValue, StateType, StateDecision, StateConsume, ValidFromUtc, ValidToUtc, IssuedUtc, LastPolling
FROM            dbo.tbl_State