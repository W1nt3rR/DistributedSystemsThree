# Stage 1: Build the Go binary
FROM golang:1.22.6-alpine AS builder

WORKDIR /app
COPY . .

# Download dependencies and build the binary
RUN go mod download
RUN go build -o main .

# Stage 2: Create a minimal runtime image
FROM alpine:latest

WORKDIR /app

# Copy the binary from the builder stage
COPY --from=builder /app/main .
COPY --from=builder /app/.env.production .env

# Expose the application port
EXPOSE 8080

# Run the binary
CMD ["./main"]