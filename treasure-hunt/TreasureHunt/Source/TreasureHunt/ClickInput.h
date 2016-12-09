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
    
    // Point light placed at claimed treasure
    UPROPERTY(EditAnywhere)
    AActor *_target;
    
    // Debug text indicating height of play area in cm
    UPROPERTY(EditAnywhere)
    AActor *_heightText;
    
    // Debug text indicating width of play area in cm
    UPROPERTY(EditAnywhere)
    AActor *_widthText;
    
    // Text indicating number of treasures collected
    UPROPERTY(EditAnywhere)
    AActor *_scoreTextActor;
    UTextRenderComponent *_scoreText;

    // Battery meter text
    UPROPERTY(EditAnywhere)
    AActor *_batteryTextActor;
    UTextRenderComponent *_batteryText;
    
    // Actor mapped to Cozmo's pose
    UPROPERTY(EditAnywhere)
    ARobotTracker *_robotTracker;
    TArray<UActorComponent *> _robotTrackerMeshes;
    
    // Actor in charge of communicating with Cozmo SDK
    UPROPERTY(EditAnywhere)
    ACozmoUE *_cozmoUE;
    
    // Sound played when battery is charged
    UPROPERTY(EditAnywhere)
    AAmbientSound *_batterySound;
    
    // Sound played when cursor hovers Cozmo or treasure spot
    UPROPERTY(EditAnywhere)
    AAmbientSound *_outlineSound;
    
    // Sound played when treasure is claimed
    UPROPERTY(EditAnywhere)
    AAmbientSound *_claimSound;
    
    // Sound played when Cozmo digs up treasure
    UPROPERTY(EditAnywhere)
    AAmbientSound *_digSound;
    
    // Callback for when action button is pressed:
    //      If Cozmo is hovered, charges Cozmo
    //      If treasure is hovered and Cozmo isn't currently approaching treasure, then treasure is claimed.
    UFUNCTION()
    void OnActionButton();
    
    // Tells Cozmo to move to claimed treasure and schedules callback for completion (OnReach)
    UFUNCTION()
    void StartApproachTreasure();
    
    // Callback for when Cozmo reaches claimed treasure spot
    UFUNCTION()
    void OnReach();
    
    // Toggles debug visuals
    UFUNCTION()
    void ToggleDebug();

    // Toggles battery depletion mechanic
    UFUNCTION()
    void ToggleBattery();
    
    // Enables or disables placing claiming of treasure
    UFUNCTION()
    void SetClaimEnabled(bool claimEnabled) { _claimEnabled = claimEnabled; }
    
    AActor *_hoveredActor;
    
    void BindControls();
    
    int _score = 0;
    float _batteryLevel = 100.0;
    float _batteryDepletionSpeed = 6.0f;
    float _minBatteryDepletionSpeed = 1.0f; // % lost per second
    float _maxBatteryDepletionSpeed = 33.0f;
    float _batteryDepletionAcceleration = .025;
    bool _didBindControls;
    bool _claimEnabled = true;
};
