################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../makefile.local

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS_QUOTED += \
"../Sources/bootloader.c" \

C_SRCS += \
../Sources/bootloader.c \

OBJS += \
./Sources/bootloader.o \

OBJS_QUOTED += \
"./Sources/bootloader.o" \

C_DEPS += \
./Sources/bootloader.d \

OBJS_OS_FORMAT += \
./Sources/bootloader.o \

C_DEPS_QUOTED += \
"./Sources/bootloader.d" \


# Each subdirectory must supply rules for building sources it contributes
Sources/bootloader.o: ../Sources/bootloader.c
	@echo 'Building file: $<'
	@echo 'Executing target #1 $<'
	@echo 'Invoking: ARM Ltd Windows GCC C Compiler'
	"$(ARMSourceryDirEnv)/arm-none-eabi-gcc" "$<" @"Sources/bootloader.args" -MMD -MP -MF"$(@:%.o=%.d)" -o"Sources/bootloader.o"
	@echo 'Finished building: $<'
	@echo ' '


