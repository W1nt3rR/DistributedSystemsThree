package main

import (
	"conference_management_service/proto"
	"conference_management_service/routers"
	"context"
	"fmt"
	"log"
	"net"
	"net/http"
	"os"

	"github.com/gin-gonic/gin"
	"github.com/joho/godotenv"
	"google.golang.org/grpc"
)

type server struct {
	proto.UnimplementedSignupServiceServer
}

func (s *server) SignupMember(ctx context.Context, req *proto.Signup) (*proto.SignupResponse, error) {
	log.Printf("gRPC Server received: %v", req)

	conferenceID := req.GetConferenceId() // Assuming conference_id is used similarly to conferenceID in original
	token := req.GetMemberJwtToken()

	baseURL := os.Getenv("CONFERENCE_MANAGEMENT_SERVICE_BASE_URL")
	url := fmt.Sprintf("%s/api/Signup/%s/Invite", baseURL, conferenceID) // Adjusted to match conference_id use

	log.Printf("url: %v", url)

	httpReq, err := http.NewRequest("POST", url, nil)
	if err != nil {
		log.Printf("Failed to create HTTP request: %v", err)
		return &proto.SignupResponse{Approved: false, Message: "Failed to create HTTP request"}, nil
	} else {
		log.Printf("Post request made: %v", httpReq)
	}

	httpReq.Header.Set("Authorization", "Bearer "+token)

	client := &http.Client{}
	resp, err := client.Do(httpReq)
	if err != nil {
		log.Printf("Failed to send HTTP request: %v", err)
		return &proto.SignupResponse{Approved: false, Message: "Failed to send HTTP request"}, nil
	} else {
		log.Printf("Post request sent: %v", httpReq)
	}

	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		log.Printf("Failed to invite member, received status code: %d", resp.StatusCode)
		return &proto.SignupResponse{Approved: false, Message: "Failed to invite member"}, nil
	} else {
		log.Printf("Member invited successfully.")
	}

	log.Printf("Member with Conference ID %s successfully invited", conferenceID)
	return &proto.SignupResponse{Approved: true, Message: "Member invited successfully"}, nil
}

func main() {
	err := godotenv.Load()
	if err != nil {
		log.Fatal("Error loading .env file")
	} else {
		log.Println("Successfully loaded .env file")
	}

	router := routers.SignupRouter()

	gin.SetMode(gin.ReleaseMode)

	port := ":8080"
	go func() {
		log.Printf("Starting HTTP server on port %s", port)
		if err := http.ListenAndServe(port, router); err != nil {
			log.Fatalf("Could not start HTTP server: %v", err)
		}
	}()

	grpcPort := ":666"
	lis, err := net.Listen("tcp", grpcPort)
	if err != nil {
		log.Fatalf("Failed to listen on %v: %v", grpcPort, err)
	} else {
		log.Printf("Listening on gRPC port: %v", grpcPort)
	}

	grpcServer := grpc.NewServer()
	proto.RegisterSignupServiceServer(grpcServer, &server{})

	log.Printf("Starting gRPC server on port %s", grpcPort)
	if err := grpcServer.Serve(lis); err != nil {
		log.Fatalf("Failed to serve gRPC server: %v", err)
	}
}
