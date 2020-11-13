

CREATE VIEW [svc].[uvw_EmailQueue]
AS
SELECT        Id, ActorId, FromId, FromEmail, FromDisplay, ToId, ToEmail, ToDisplay, Subject, HtmlContent, PlaintextContent, CreatedUtc, SendAtUtc
FROM            [dbo].[tbl_EmailQueue]