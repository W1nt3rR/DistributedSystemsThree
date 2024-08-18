package models

import (
	"go.mongodb.org/mongo-driver/bson/primitive"
)

type Signup struct {
	ID           primitive.ObjectID `bson:"_id,omitempty" json:"id"`
	ConferenceID string             `bson:"conferenceId" json:"conferenceId"`
	CreatedAt    primitive.DateTime `bson:"createdAt" json:"createdAt"`
	UpdatedAt    primitive.DateTime `bson:"updated_at"`
	MemberEmails []string           `bson:"memberEmails" json:"memberEmails"`
}
