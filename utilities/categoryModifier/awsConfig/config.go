package awsConfig

import (
	"context"

	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/credentials"
)

func GetConfig(environment string) aws.Config {
	if environment == "dev" {
		cfg, _ := config.LoadDefaultConfig(context.TODO(), config.WithRegion("ap-southeast-2"),
			config.WithEndpointResolverWithOptions(aws.EndpointResolverWithOptionsFunc(func(_, _ string, _ ...interface{}) (aws.Endpoint, error) {
				return aws.Endpoint{
					URL:           "http://localhost:4566",
					SigningRegion: "ap-southeast-2",
				}, nil
			})),
			config.WithCredentialsProvider(credentials.NewStaticCredentialsProvider("dummy", "dummy", "dummy")))
		return cfg

	} else {
		cfg, err := config.LoadDefaultConfig(context.TODO(), config.WithRegion("ap-southeast-2"))
		if err != nil {
			panic(err)
		}
		return cfg
	}

}
