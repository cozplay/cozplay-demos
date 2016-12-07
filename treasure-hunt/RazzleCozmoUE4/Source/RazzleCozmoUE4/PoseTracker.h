// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CozmoUE.h"
#include "GameFramework/Actor.h"
#include "PoseTracker.generated.h"

// Base class for Actors that track poses from the Cozmo SDK
UCLASS(abstract)
class RAZZLECOZMOUE4_API APoseTracker : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	APoseTracker();

	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void Tick( float DeltaSeconds ) override;

protected:
    FCozmoPoseStruct _pose;
    
    // Fetches the pose associated with this object from the Cozmo SDK
    virtual FCozmoPoseStruct FetchPose();
    
    /** CozmoUE instance associated with this tracker */
    UPROPERTY(EditAnywhere)
    ACozmoUE *_cozmoUE;
    
    /** Should we track x position of robot? */
    UPROPERTY(EditAnywhere)
    bool _trackPosX = true;
    
    /** Should we track y position of robot? */
    UPROPERTY(EditAnywhere)
    bool _trackPosY = true;
    
    /** Should we track z position of robot? */
    UPROPERTY(EditAnywhere)
    bool _trackPosZ = true;
    
    /** Should we track free rotation of robot? */
    UPROPERTY(EditAnywhere)
    bool _trackRotation = true;
    
    /** Should we track z angle of robot? */
    UPROPERTY(EditAnywhere)
    bool _trackZAngle = true;
};
