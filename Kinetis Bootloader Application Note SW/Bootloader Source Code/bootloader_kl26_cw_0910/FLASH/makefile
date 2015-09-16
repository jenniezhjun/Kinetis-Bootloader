################################################################################
# Automatically-generated file. Do not edit!
################################################################################

#Progress monitor hint: 6
first : all
-include ../makefile.init

# This file contains definitions of environment variables used in the makefiles and .args files if exist.
-include makefile.local

RM := "$(GNU_Make_Install_DirEnv)/rm" -rf

# All of the sources participating in the build are defined here
-include sources.mk
-include subdir.mk
-include Sources/subdir.mk
-include Project_Settings/Startup_Code/subdir.mk
-include Driver/rs232/subdir.mk
-include Driver/flash/subdir.mk
-include objects.mk

ifneq ($(MAKECMDGOALS),clean)
ifneq ($(strip $(C_DEPS)),)
-include $(C_DEPS)
endif
ifneq ($(strip $(ASM_DEPS)),)
-include $(ASM_DEPS)
endif
ifneq ($(strip $(ASM_UPPER_DEPS)),)
-include $(ASM_UPPER_DEPS)
endif
ifneq ($(strip $(SX_DEPS)),)
-include $(SX_DEPS)
endif
ifneq ($(strip $(S_DEPS)),)
-include $(S_DEPS)
endif
ifneq ($(strip $(S_UPPER_DEPS)),)
-include $(S_UPPER_DEPS)
endif
endif

-include ../makefile.defs

# Add inputs and outputs from these tool invocations to the build variables 
EXECUTABLES += \
bootloader_kl26_cw.elf \

EXECUTABLES_QUOTED += \
"bootloader_kl26_cw.elf" \

EXECUTABLES_OS_FORMAT += \
bootloader_kl26_cw.elf \

SECONDARY_FLASH += \
bootloader_kl26_cw.hex \

SECONDARY_FLASH_QUOTED += \
"bootloader_kl26_cw.hex" \

SECONDARY_FLASH_OS_FORMAT += \
bootloader_kl26_cw.hex \

SECONDARY_SIZE += \
bootloader_kl26_cw.siz \

SECONDARY_SIZE_QUOTED += \
"bootloader_kl26_cw.siz" \

SECONDARY_SIZE_OS_FORMAT += \
bootloader_kl26_cw.siz \


# All Target
all: warning-messages explicit-dependencies bootloader_kl26_cw.elf secondary-outputs

# Tool invocations
bootloader_kl26_cw.elf: $(OBJS) $(USER_OBJS) ../Project_Settings/Linker_Files/MKL26Z128_flash.ld
	@echo 'Building target: $@'
	@echo 'Executing target #7 $@'
	@echo 'Invoking: ARM Ltd Windows GCC C Linker'
	"$(ARMSourceryDirEnv)/arm-none-eabi-gcc"  $(USER_OBJS_QUOTED) $(LIBS_QUOTED) @"bootloader_kl26_cw.args" -o"bootloader_kl26_cw.elf"
	@echo 'Finished building target: $@'
	@echo ' '

bootloader_kl26_cw.hex: bootloader_kl26_cw.elf
	@echo 'Executing target #8 $@'
	@echo 'Invoking: ARM Ltd Windows GNU Create Flash Image'
	"$(ARMSourceryDirEnv)/arm-none-eabi-objcopy"  -O srec bootloader_kl26_cw.elf "bootloader_kl26_cw.hex"
	@echo 'Finished building: $@'
	@echo ' '

bootloader_kl26_cw.siz: bootloader_kl26_cw.elf
	@echo 'Executing target #9 $@'
	@echo 'Invoking: ARM Ltd Windows GNU Print Size'
	"$(ARMSourceryDirEnv)/arm-none-eabi-size"  --format=berkeley bootloader_kl26_cw.elf
	@echo 'Finished building: $@'
	@echo ' '

# Other Targets
clean:
	-$(RM) $(S_DEPS_QUOTED) $(SECONDARY_FLASH_QUOTED) "./*/*/*.o" "./*/*.o"  $(S_UPPER_DEPS_QUOTED) $(ASM_DEPS_QUOTED) $(SECONDARY_SIZE_QUOTED) $(ASM_UPPER_DEPS_QUOTED) $(SX_DEPS_QUOTED) $(EXECUTABLES_QUOTED) "./*/*/*.d" "./*/*.d"  $(ProjDirPath)/FLASH/bootloader_kl26_cw.elf
	-@echo ' '

secondary-outputs: $(SECONDARY_FLASH) $(SECONDARY_SIZE)

.PHONY: all clean dependents explicit-dependencies warning-messages
.SECONDARY:

-include ../makefile.targets

explicit-dependencies: ../Project_Settings/Linker_Files/MKL26Z128_flash.ld bootloader_kl26_cw.elf bootloader_kl26_cw.elf 

warning-messages: 

