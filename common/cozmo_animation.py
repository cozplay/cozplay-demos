import random

'''
Class for filtering cozmo animations in different groups
@class CozmoAnimation
@author - Team Cozplay
'''


class CozmoAnimation:
    def get_random_pos_anim(self):
        pos_anims = [
            "majorwin",
            "anim_pounce_success_01",
            "anim_pounce_success_02",
            "anim_meetcozmo_celebration_02"
        ]

        return random.choice(pos_anims)

    def get_random_neg_anim(self):
        neg_anims = [
            "ID_pokedB",
            "majorfail",
            "anim_pounce_fail_01",
            "anim_pounce_fail_02",
            "anim_bored_event_01",
            "ID_test_shiver",
            "anim_gotosleep_off_01",
            "anim_gotosleep_getin_01"
            "id_rollblock_fail_01"

        ]

        return random.choice(neg_anims)

    def get_random_idle_anim(self):
        idle_anims = [
            "anim_hiking_react_04",
            "anim_explorer_driving01_start_01",
            "anim_explorer_driving01_start_02",
            "anim_explorer_driving01_turbo_01",
            "anim_explorer_drvback_start_01",
            "anim_explorer_idle_01",
            "anim_explorer_idle_02",
            "anim_explorer_idle_03",
            "anim_sparking_driving_loop_01",
            "anim_sparking_driving_loop_02",
            "anim_sparking_driving_loop_03",

        ]

        return random.choice(idle_anims)

    def get_random_react_anim(self):
        idle_anims = [
            "anim_reacttoblock_react_short_01"
            "anim_reacttoblock_react_short_02"
            "anim_hiking_react_04",
            "anim_explorer_driving01_start_01",
            "anim_explorer_driving01_start_02",
            "anim_explorer_driving01_turbo_01",
            "anim_explorer_drvback_start_01",
            "anim_explorer_idle_01",
            "anim_explorer_idle_02",
            "anim_explorer_idle_03",
            "anim_sparking_driving_loop_01",
            "anim_sparking_driving_loop_02",
            "anim_sparking_driving_loop_03",

        ]

        return random.choice(idle_anims)
