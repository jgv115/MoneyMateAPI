CREATE VIEW user_profiles (userId, user_identifier, profile_id, profile_name)
AS
SELECT u.id, u.user_identifier, up.profile_id, p.display_name
from users u
         JOIN userprofile up ON u.id = up.user_id
         JOIN profile p ON p.id = up.profile_id