package routers

import (
	signupController "conference_management_service/controllers"
	"conference_management_service/middleware"

	"github.com/gin-gonic/gin"
)

func SignupRouter() *gin.Engine {
	router := gin.Default()

	router.Use(middleware.JWTMiddleware())

	signupGroup := router.Group(BaseRoute)
	{
		signupGroup.POST("/", middleware.RequireRoles("Member"), signupController.CreateSignup)
		signupGroup.GET("/:id", middleware.RequireRoles("Member"), signupController.GetSignupByID)
		signupGroup.PUT("/:id", middleware.RequireRoles("Member"), signupController.UpdateSignup)
		signupGroup.DELETE("/:id", middleware.RequireRoles("Member"), signupController.DeleteSignup)
		signupGroup.GET("/", middleware.RequireRoles("Admin"), signupController.GetAllSignups)
		signupGroup.POST("/:conferenceId/Invite", middleware.RequireRoles("Member"), signupController.AddMemberToSignup)
	}

	return router
}
