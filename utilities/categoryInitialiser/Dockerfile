#FROM public.ecr.aws/lambda/provided:al2 as build
#
#ADD . .
#
#RUN yum install -y tar gzip git
#RUN curl -O https://storage.googleapis.com/golang/go1.16.linux-amd64.tar.gz && \
#    tar -xvf go1.16.linux-amd64.tar.gz && \
#    chown -R root:root go && \
#    mv go /usr/local
#
#ENV PATH="/usr/local/go/bin:${PATH}"
#
#
#RUN go env -w GOPROXY=direct
#ADD go.mod go.sum ./
#RUN go mod download
#RUN GOOS=linux go build -o /main main.go
## RUN export PATH=$PATH:/usr/local/go/bin
#
#
#### install compiler
###RUN yum install -y golang
###RUN go env -w GOPROXY=direct
##
### cache dependencies
##ADD go.mod go.sum ./
##RUN go mod download
##
### build
##ADD . .
###RUN go build -o /main
##RUN GOOS=linux go build -o /main main.go
#
### copy artifacts to a clean image
##FROM public.ecr.aws/lambda/provided:al2
##COPY --from=build /main /main
##ENTRYPOINT [ "/main" ]

FROM golang:latest as build

WORKDIR /app

COPY go.mod go.sum ./

RUN go mod download

COPY . .

RUN GOOS=linux CGO_ENABLED=0 go build -o /main .

FROM public.ecr.aws/lambda/provided:al2023
COPY --from=build /main /main
ENTRYPOINT [ "/main" ]