// Fill out your copyright notice in the Description page of Project Settings.

#include "TreasureHunt.h"
#include "ClickInput.h"
#include "UT.h"


// Sets default values for this component's properties
UClickInput::UClickInput()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	bWantsBeginPlay = true;
	PrimaryComponentTick.bCanEverTick = true;

	// ...
}


// Called when the game starts
void UClickInput::BeginPlay()
{
	Super::BeginPlay();
    _robotTrackerMeshes = _robotTracker->GetComponentsByTag(UStaticMeshComponent::StaticClass(), TEXT("Debug"));
    _scoreText = _scoreTextActor->FindComponentByClass<UTextRenderComponent>();
    _batteryText = _batteryTextActor->FindComponentByClass<UTextRenderComponent>();
    if (GetOwner()->InputComponent != NULL) {
        BindControls();
        _didBindControls = true;
    }
}


// Called every frame
void UClickInput::TickComponent( float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction )
{
	Super::TickComponent( DeltaTime, TickType, ThisTickFunction );
    if (!_didBindControls && GetOwner()->InputComponent != NULL) {
        BindControls();
        _didBindControls = true;
    }
    
    // Raycast down
	FHitResult hit;
    UT::Trace(GetWorld(), GetOwner(), GetOwner()->GetActorLocation(), FVector(0,0,-1), 17.330173, hit);
    AActor *hitActor = hit.Actor.Get();
    
    bool isNewActor = hitActor != _hoveredActor;
    bool isHitTreasure = (hitActor != NULL) && hitActor->IsA(ATreasure::StaticClass());
    bool isHitRobot = (hitActor != NULL) && hitActor->IsA(ARobotTracker::StaticClass());
    bool isHoveredTreasure = (_hoveredActor != NULL) && _hoveredActor->IsA(ATreasure::StaticClass());
    bool isHoveredRobot = (_hoveredActor != NULL) && _hoveredActor->IsA(ARobotTracker::StaticClass());
    
    if (isNewActor) {
        // Remove outline from previous hovered actor
        if (_hoveredActor != NULL) {
            if (isHoveredTreasure) {
                ((ATreasure *)_hoveredActor)->ShowOutline(false);
            } else if (isHoveredRobot) {
                ((ARobotTracker *)_hoveredActor)->ShowOutline(false);
            }
        }
        _hoveredActor = NULL;
        // Add outline to new hovered actor, if we're not claiming anything currently
        if (hitActor != NULL) {
            // NOTE: _baitEnabled was not properly reenabled always, possibly related to Cozmo go_to_pose not finishing and losing
            // Cozmo's world. But with changes for softs, it shouldn't be necessary
            if (isHitTreasure && ATreasure::ClaimedTreasure() == NULL && ((ATreasure *)hitActor)->IsActive() /*&& _baitEnabled*/) {
                ((ATreasure *)hitActor)->ShowOutline(true);
                _outlineSound->Play();
                _hoveredActor = hitActor;
            } else if (isHitRobot) {
                _outlineSound->Play();
                ((ARobotTracker *)hitActor)->ShowOutline(true);
                _hoveredActor = hitActor;
            }
        }
    } else if (isHitTreasure) {
        if (!((ATreasure *)_hoveredActor)->IsActive()) {
            _hoveredActor = NULL;
            //UE_LOG(LogTemp, Warning, TEXT("WASN'T ACTIVE"));
        } else if (_baitEnabled) {
            ((ATreasure *)_hoveredActor)->ShowOutline(((ATreasure *)_hoveredActor)->IsActive());
            //UE_LOG(LogTemp, Warning, TEXT("BAIT ENABLED"));
        } else {
            //UE_LOG(LogTemp, Warning, TEXT("BAIT DISABLED"));
        }
    } else if (isHitRobot) {
        ((ARobotTracker *)_hoveredActor)->ShowOutline(true);
    }
    
    _batteryDepletionSpeed = FMath::Min(_maxBatteryDepletionSpeed, _batteryDepletionSpeed + _batteryDepletionAcceleration * DeltaTime);
    _batteryLevel -= DeltaTime * _batteryDepletionSpeed;
    _batteryText->SetText(FString::Printf(TEXT("|||%d%%"), (int)_batteryLevel));
    if (_batteryLevel > 80.0) {
        _batteryText->SetTextRenderColor(FColor(255, 255, 255));
    } else if (_batteryLevel < 20.0) {
        _batteryText->SetTextRenderColor(FColor(255, 0, 0));
    } else {
        _batteryText->SetTextRenderColor(FColor(121, 98, 34));
    }
    // TEMP
    if (_batteryLevel <= 0.0f) {
        _batteryText->SetText("GAME OVER");
    }
}

void UClickInput::OnClick()
{
    if (_hoveredActor == NULL) {
        return;
    }
    
    if(_hoveredActor->IsA(ARobotTracker::StaticClass())) {
        _batteryLevel = 100.0f;
        _batteryText->SetText(FString::Printf(TEXT("|||100%%")));
        _batterySound->Play();
    } else if (_hoveredActor->IsA(ATreasure::StaticClass())) {
        if (((ATreasure *)_hoveredActor)->AttemptClaim()) {
            PlaceBait();
        }
    }
}

void UClickInput::PlaceBait()
{
    UE_LOG(LogTemp, Warning, TEXT("CLICKED"));
    _baitEnabled = false;
    FVector targetLocation = _target->GetActorLocation();
    
    if (_hoveredActor != NULL && _hoveredActor == ATreasure::ClaimedTreasure()) {
        // Move to center of treasure if touching tresure
        FVector location = ATreasure::ClaimedTreasure()->GetActorLocation();
        targetLocation.X = location.X;
        targetLocation.Y = location.Y;
        _target->SetActorLocation(targetLocation);
        _cozmoUE->ForceGoToPosition(targetLocation.X * 10.0, targetLocation.Y * -10.0, this, TEXT("OnReach"));
        _claimSound->Play();
    }
}

void UClickInput::OnReach()
{
    ATreasure::ClaimedTreasure()->OnReached();
    _cozmoUE->RunCozmoCoroutine("self.on_reach()", this, TEXT("SetBaitEnabled true"));
    _score++;
    _scoreText->SetText(FString::Printf(TEXT("%d"), _score));
    _digSound->Play();
}

void UClickInput::ToggleDebug()
{
    bool hidden = !_widthText->bHidden;
    _widthText->SetActorHiddenInGame(hidden);
    _heightText->SetActorHiddenInGame(hidden);
    for (UActorComponent *mesh : _robotTrackerMeshes) {
        ((UStaticMeshComponent *)mesh)->SetVisibility(!hidden);
    }
} 

void UClickInput::ToggleBattery()
{
    _batteryText->GetOwner()->SetActorHiddenInGame(!_batteryText->GetOwner()->bHidden);
}

void UClickInput::BindControls()
{
    UInputComponent *inputComponent = GetOwner()->InputComponent;
    inputComponent->BindAction("PlaceBait", IE_Pressed, this, &UClickInput::OnClick);
    inputComponent->BindAction("ToggleDebug", IE_Pressed, this, &UClickInput::ToggleDebug);
    inputComponent->BindAction("ToggleBattery", IE_Pressed, this, &UClickInput::ToggleBattery);
}
