Google Drive File System Settings (for Unit Tests)

The GoogleDriveFileSystem uses a service account, which is an account that belongs to an 
application instead of to an individual end user. It calls Google APIs on behalf 
of the service account, so users aren't directly involved.

A service account's credentials include a generated email, a client ID, and at 
least one public/private key pair. You must obtain these credentials in the Google 
Developers Console. To generate service-account credentials do the following:

1. Open the Credentials page https://console.developers.google.com/project/_/apiui/credential.
2. Select or create a project.
3. Click Add credentials > Service account. 
4. Choose to download the service account's public/private key as a standard P12 file. 
6. Set the NFX_GOOGLE_DRIVE environment variable to "google-drive{ email='<service account's email>' cert-path=$'<path to P12 file' }".



Andrey Kolbasov <andrey@kolbasov.com>