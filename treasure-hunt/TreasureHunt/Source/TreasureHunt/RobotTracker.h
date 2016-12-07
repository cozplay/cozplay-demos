// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "PoseTracker.h"
#include "GameFramework/Actor.h"
#include "RobotTracker.generated.h"

UCLASS()
class TREASUREHUNT_API ARobotTracker : public APoseTracker
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	ARobotTracker();

	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void Tick( float DeltaSeconds ) override;
    
    // Shows or hides circle outlining around Cozmo
    UFUNCTION()
    void ShowOutline(bool shouldShow);
    
protected:
    virtual FCozmoPoseStruct FetchPose() override;
    
private:
    UStaticMeshComponent *_outline;
};
