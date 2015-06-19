Set oWebAdmin = GetObject("winmgmts:root\WebAdministration")

' Define the Path, SiteName, and PhysicalPath for the new application.

strApplicationPath = "/Validation"
strSiteName = "Default Web Site"
strPhysicalPath = "C:\inetpub\wwwroot\Validation"

' Create the new application

oWebAdmin.Get("Application").Create strApplicationPath, strSiteName, strPhysicalPath