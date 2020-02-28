
CREATE VIEW [svc].[uvw_States]
AS
SELECT        Id, IssuerId, AudienceId, UserId, StateValue, StateType, StateDecision, StateConsume, ValidFromUtc, ValidToUtc, IssuedUtc, LastPolling
FROM            dbo.tbl_States