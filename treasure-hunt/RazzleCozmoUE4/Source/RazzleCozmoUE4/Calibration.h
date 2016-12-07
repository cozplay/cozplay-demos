// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Components/ActorComponent.h"
#include "Calibration.generated.h"


UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class RAZZLECOZMOUE4_API UCalibration : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UCalibration();

	// Called when the game starts
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void TickComponent( float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction ) override;

private:
    bool _didBindControls;
    void BindControls();
	
    UFUNCTION()
    void LoadGame();
};
