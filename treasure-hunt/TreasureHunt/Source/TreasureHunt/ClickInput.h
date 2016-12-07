// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CozmoUE.h"
#include "RobotTracker.h"
#include "Treasure.h"
#include "Components/ActorComponent.h"
#include "ClickInput.generated.h"


// TODO: Restructure into multiple classes... Most of the game logic ended up here.
UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class TREASUREHUNT_API UClickInput : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UClickInput();

	// Called when the game starts
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void TickComponent( float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction ) override;

private:
    UInputComponent *inputComponent;
    
    UPROPERTY(EditAnywhere)
    AActor *_target;
    
    UPROPERTY(EditAnywhere)
    AActor *_heightText;
    
    UPROPERTY(EditAnywhere)
    AActor *_widthText;
    
    UPROPERTY(EditAnywhere)
    AActor *_scoreTextActor;
    UTextRenderComponent *_scoreText;

    UPROPERTY(EditAnywhere)
    AActor *_batteryTextActor;
    UTextRenderComponent *_batteryText;
    
    UPROPERTY(EditAnywhere)
    ARobotTracker *_robotTracker;
    TArray<UActorComponent *> _robotTrackerMeshes;
    
    UPROPERTY(EditAnywhere)
    ACozmoUE *_cozmoUE;
    
    UPROPERTY(EditAnywhere)
    AAmbientSound *_batterySound;
    
    UPROPERTY(EditAnywhere)
    AAmbientSound *_outlineSound;
    
    UPROPERTY(EditAnywhere)
    AAmbientSound *_claimSound;
    
    UPROPERTY(EditAnywhere)
    AAmbientSound *_digSound;
    
    UPROPERTY(EditAnywhere)
    AAmbientSound *_baitSound;
    
    UFUNCTION()
    void OnClick();
    
    UFUNCTION()
    void PlaceBait();
    
    UFUNCTION()
    void OnReach();
    
    UFUNCTION()
    void ToggleDebug();

    UFUNCTION()
    void ToggleBattery();
    
    UFUNCTION()
    void SetBaitEnabled(bool baitEnabled) { _baitEnabled = baitEnabled; }
    
    AActor *_hoveredActor;
    
    void BindControls();
    
    int _score = 0;
    float _batteryLevel = 100.0;
    float _batteryDepletionSpeed = 6.0f;
    float _minBatteryDepletionSpeed = 1.0f; // % lost per second
    float _maxBatteryDepletionSpeed = 33.0f;
    float _batteryDepletionAcceleration = .025;
    bool _didBindControls;
    bool _baitEnabled = true;
};
