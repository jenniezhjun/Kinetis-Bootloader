################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../../makefile.local

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS_QUOTED += \
"../Driver/rs232/rs232.c" \

C_SRCS += \
../Driver/rs232/rs232.c \

OBJS += \
./Driver/rs232/rs232.o \

OBJS_QUOTED += \
"./Driver/rs232/rs232.o" \

C_DEPS += \
./Driver/rs232/rs232.d \

OBJS_OS_FORMAT += \
./Driver/rs232/rs232.o \

C_DEPS_QUOTED += \
"./Driver/rs232/rs232.d" \


# Each subdirectory must supply rules for building sources it contributes
Driver/rs232/rs232.o: ../Driver/rs232/rs232.c
	@echo 'Building file: $<'
	@echo 'Executing target #5 $<'
	@echo 'Invoking: ARM Ltd Windows GCC C Compiler'
	"$(ARMSourceryDirEnv)/arm-none-eabi-gcc" "$<" @"Driver/rs232/rs232.args" -MMD -MP -MF"$(@:%.o=%.d)" -o"Driver/rs232/rs232.o"
	@echo 'Finished building: $<'
	@echo ' '


