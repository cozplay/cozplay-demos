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
    
    // Raycast down to update hover state
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
        // Add outline to new hovered actor, if we're not moving toward claimed treasure
        if (hitActor != NULL) {
            if (isHitTreasure && ATreasure::ClaimedTreasure() == NULL && ((ATreasure *)hitActor)->IsActive()) {
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
        } else if (_claimEnabled) {
            ((ATreasure *)_hoveredActor)->ShowOutline(((ATreasure *)_hoveredActor)->IsActive());
        }
    } else if (isHitRobot) {
        ((ARobotTracker *)_hoveredActor)->ShowOutline(true);
    }
    
    // Update battery meter
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
    
    // GAME OVER MESSAGE
    if (_batteryLevel <= 0.0f) {
        _batteryText->SetText("GAME OVER");
    }
}

void UClickInput::OnActionButton()
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
            StartApproachTreasure();
        }
    }
}

void UClickInput::StartApproachTreasure()
{
    UE_LOG(LogTemp, Warning, TEXT("CLICKED"));
    _claimEnabled = false;
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
    _cozmoUE->RunCozmoCoroutine("self.on_reach()", this, TEXT("SetClaimEnabled true"));
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
    inputComponent->BindAction("ActionButton", IE_Pressed, this, &UClickInput::OnActionButton);
    inputComponent->BindAction("ToggleDebug", IE_Pressed, this, &UClickInput::ToggleDebug);
    inputComponent->BindAction("ToggleBattery", IE_Pressed, this, &UClickInput::ToggleBattery);
}
