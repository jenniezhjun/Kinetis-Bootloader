################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../../makefile.local

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS_QUOTED += \
"../Driver/flash/flash_kinetis.c" \

C_SRCS += \
../Driver/flash/flash_kinetis.c \

OBJS += \
./Driver/flash/flash_kinetis.o \

OBJS_QUOTED += \
"./Driver/flash/flash_kinetis.o" \

C_DEPS += \
./Driver/flash/flash_kinetis.d \

OBJS_OS_FORMAT += \
./Driver/flash/flash_kinetis.o \

C_DEPS_QUOTED += \
"./Driver/flash/flash_kinetis.d" \


# Each subdirectory must supply rules for building sources it contributes
Driver/flash/flash_kinetis.o: ../Driver/flash/flash_kinetis.c
	@echo 'Building file: $<'
	@echo 'Executing target #6 $<'
	@echo 'Invoking: ARM Ltd Windows GCC C Compiler'
	"$(ARMSourceryDirEnv)/arm-none-eabi-gcc" "$<" @"Driver/flash/flash_kinetis.args" -MMD -MP -MF"$(@:%.o=%.d)" -o"Driver/flash/flash_kinetis.o"
	@echo 'Finished building: $<'
	@echo ' '


