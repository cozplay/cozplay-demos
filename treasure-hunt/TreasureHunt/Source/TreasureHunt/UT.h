// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

/**
 * Class containing static raycast functions
 * source: https://forums.unrealengine.com/showthread.php?94448-Anyone-know-where-I-can-find-a-simple-raycast-with-trace-example-in-C
 */
class TREASUREHUNT_API UT
{
public:
    static bool Trace(UWorld* world, AActor* actorToIgnore, const FVector& start, const FVector& dir, float length,
                      FHitResult& hit, ECollisionChannel CollisionChannel = ECC_Pawn, bool ReturnPhysMat = false);
    static bool Trace(UWorld* world, AActor* actorToIgnore, const FVector& start, const FVector& end, FHitResult& hit,
                      ECollisionChannel CollisionChannel = ECC_Pawn, bool ReturnPhysMat = false);
};
