package aws_utils

import (
	"fmt"
	"os"

	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
)

func CreateAWSSession(environment string) (sess *session.Session) {
	if environment == "dev" {
		localstackHostname, localstackHostnameSet := os.LookupEnv("LOCALSTACK_HOSTNAME")

		if !localstackHostnameSet {
			localstackHostname = "localhost"
		}

		fmt.Printf("using localstack hostname of: %v", localstackHostname)

		sess = session.Must(session.NewSession(&aws.Config{
			Endpoint: aws.String(fmt.Sprintf("http://%s:4566", localstackHostname)),
			Region:   aws.String("ap-southeast-2"),
		}))
	} else {
		sess = session.Must(session.NewSessionWithOptions(session.Options{
			SharedConfigState: session.SharedConfigDisable,
		}))
	}
	return
}
