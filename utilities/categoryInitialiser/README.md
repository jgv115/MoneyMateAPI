# MoneyMate Category Initialiser

This Lambda is a utility dedicated to initialising categories and subcategories for a user once they register for MoneyMate.

## Tech notes
- The Terraform in this utility will contain the code for a hook that will run in Auth0
- The Terraform in this utility contains an IAM User that needs to be manually populated into Auth0 to run the hook