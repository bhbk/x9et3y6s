
CREATE VIEW [svc].[uvw_QueueEmails]
AS
SELECT        Id, ActorId, FromId, FromEmail, FromDisplay, ToId, ToEmail, ToDisplay, Subject, HtmlContent, PlaintextContent, Created, SendAt
FROM            dbo.tbl_QueueEmails