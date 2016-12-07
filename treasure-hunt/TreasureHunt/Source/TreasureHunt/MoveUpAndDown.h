// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Components/ActorComponent.h"
#include "MoveUpAndDown.generated.h"


// Just moves an actor up and down. This is just to test that Cozmo isn't blocking the main thread.
UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class TREASUREHUNT_API UMoveUpAndDown : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UMoveUpAndDown();

	// Called when the game starts
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void TickComponent( float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction ) override;

private:
    float _speed = 100.0f;
    float _elapsed = 0.0f;
    float _amplitude = 100.0f;
    float _initialZ = 0.0f;
};
