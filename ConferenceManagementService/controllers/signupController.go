package signupController

import (
	"conference_management_service/models"
	"context"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/joho/godotenv"
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

var signupCollection *mongo.Collection

func init() {
	err := godotenv.Load(".env")
	if err != nil {
		log.Fatalf("Error loading .env file")
	}

	connectionString := os.Getenv("MONGODB_CONNECTION_STRING")
	fmt.Println("Connection string: " + connectionString)

	clientOptions := options.Client().ApplyURI(connectionString)

	client, err := mongo.Connect(context.TODO(), clientOptions)
	if err != nil {
		log.Fatal(err)
	}

	err = client.Ping(context.TODO(), nil)
	if err != nil {
		log.Fatal(err)
	}

	fmt.Println("Mongodb connection success")

	dbName := os.Getenv("DB_NAME")
	colName := "signups"

	signupCollection = client.Database(dbName).Collection(colName)

	fmt.Println("Collection instance is ready")
}

func CreateSignup(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	var newSignup models.Signup
	if err := c.BindJSON(&newSignup); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	newSignup.ID = primitive.NewObjectID()
	newSignup.CreatedAt = primitive.NewDateTimeFromTime(time.Now())
	newSignup.UpdatedAt = newSignup.CreatedAt

	_, err := signupCollection.InsertOne(ctx, newSignup)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to create signup"})
		return
	}

	c.JSON(http.StatusCreated, newSignup)
}

func GetSignupByID(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	id := c.Param("id")
	objID, err := primitive.ObjectIDFromHex(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid ID"})
		return
	}

	var signup models.Signup
	err = signupCollection.FindOne(ctx, bson.M{"_id": objID}).Decode(&signup)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to fetch signup"})
		return
	}

	c.JSON(http.StatusOK, signup)
}

func UpdateSignup(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	id := c.Param("id")
	objID, err := primitive.ObjectIDFromHex(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid ID"})
		return
	}

	var updateData bson.M
	if err := c.BindJSON(&updateData); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	updateData["updatedAt"] = primitive.NewDateTimeFromTime(time.Now())

	_, err = signupCollection.UpdateOne(ctx, bson.M{"_id": objID}, bson.M{"$set": updateData})
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to update signup"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Signup updated successfully"})
}

func DeleteSignup(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	id := c.Param("id")
	objID, err := primitive.ObjectIDFromHex(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid ID"})
		return
	}

	_, err = signupCollection.DeleteOne(ctx, bson.M{"_id": objID})
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to delete signup"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Signup deleted successfully"})
}

func GetAllSignups(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	cursor, err := signupCollection.Find(ctx, bson.M{})
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to fetch signups"})
		return
	}
	defer cursor.Close(ctx)

	var signups []models.Signup
	if err = cursor.All(ctx, &signups); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to parse signups"})
		return
	}

	c.JSON(http.StatusOK, signups)
}

func AddMemberToSignup(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	// Get the email from the JWT token
	email, exists := c.Get("email")
	if !exists {
		log.Printf("Email not found in token")
		c.JSON(http.StatusUnauthorized, gin.H{"error": "Email not found in token"})
		return
	}

	emailStr, ok := email.(string)
	if !ok {
		log.Printf("Failed to parse email from token")
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to parse email from token"})
		return
	}

	// Get the conferenceId from the URL parameters
	conferenceID := c.Param("conferenceId")

	// Find the signup document by conferenceId
	var signup models.Signup
	err := signupCollection.FindOne(ctx, bson.M{"conferenceId": conferenceID}).Decode(&signup)
	if err != nil {
		if err == mongo.ErrNoDocuments {
			// Create a new signup
			newSignup := models.Signup{
				ID:           primitive.NewObjectID(),
				ConferenceID: conferenceID,
				MemberEmails: []string{emailStr},
				CreatedAt:    primitive.NewDateTimeFromTime(time.Now()),
				UpdatedAt:    primitive.NewDateTimeFromTime(time.Now()),
			}

			_, err := signupCollection.InsertOne(ctx, newSignup)
			if err != nil {
				log.Printf("Failed to create new signup")
				c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to create new signup"})
				return
			}

			c.JSON(http.StatusOK, gin.H{
				"message": "Member with email " + emailStr + " has been added to new signup for conference ID " + conferenceID,
			})
			return
		} else {
			log.Printf("Failed to find signup")
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to find signup"})
			return
		}
	}

	// Update the memberEmails list and update the CreatedAt and UpdatedAt fields
	update := bson.M{
		"$push": bson.M{"memberEmails": emailStr},
		"$set":  bson.M{"updatedAt": primitive.NewDateTimeFromTime(time.Now())},
	}

	_, err = signupCollection.UpdateOne(ctx, bson.M{"_id": signup.ID}, update)
	if err != nil {
		log.Printf("Failed to add member to signup")
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to add member to signup"})
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "Member with email " + emailStr + " has been added to signup for conference ID " + conferenceID,
	})
}
