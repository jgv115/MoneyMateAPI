package models

type IntialiseCategoriesRequest struct {
	Id                  string `json:"id" validate:"required"`
	Tenant              string `json:"tenant"`
	Username            string `json:"username" validate:"required"`
	Email               string `json:"email"`
	EmailVerified       bool   `json:"emailVerified"`
	PhoneNumber         string `json:"phoneNumber"`
	PhoneNumberVerified bool   `json:"phoneNumberVerified"`
	UserMetadata        struct {
		Hobby string `json:"hobby"`
	} `json:"user_metadata"`
	AppMetadata struct {
		Plan string `json:"plan"`
	} `json:"app_metadata"`
}
