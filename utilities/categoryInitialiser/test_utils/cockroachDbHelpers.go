package test_utils

import (
	"context"

	"github.com/jackc/pgx/v5"
)

type CockroachDbHelpers struct {
	Connection *pgx.Conn
}

func (c *CockroachDbHelpers) ClearData() error {
	_, err := c.Connection.Exec(context.Background(), "TRUNCATE users, profile, category, subcategory, payerpayee CASCADE")
	return err
}

func (c *CockroachDbHelpers) CreateUser(userIdentifier string) (createdUserId string, err error) {
	err = c.Connection.QueryRow(context.Background(), `INSERT INTO users (user_identifier) VALUES ($1) RETURNING id`, userIdentifier).Scan(&createdUserId)
	return
}

func (c *CockroachDbHelpers) CreateProfile(profileName string) (createdProfileId string, err error) {

	err = c.Connection.QueryRow(context.Background(), `INSERT INTO profile (display_name) VALUES ($1) RETURNING id`, profileName).Scan(&createdProfileId)
	return
}
