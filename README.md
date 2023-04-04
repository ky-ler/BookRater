# BookRater
Web app that allows you to rate books you've read using CRUD. Users must be logged in, and cannot update other users ratings.

## How It's Made:

**Tech used:** ASP.NET Core, .NET 7.0

## Lessons Learned:
* Customize ASP.NET Core Identity (used here to change the default login/registration to use username instead of email)
* How to use IValidatableObject to validate data (used here to validate that dates are in the past, and started reading dates are before finished reading dates)
