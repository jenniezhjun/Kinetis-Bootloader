################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../makefile.local

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS_QUOTED += \
"../Sources/bl_communication.c" \
"../Sources/main.c" \
"../Sources/sa_mtb.c" \

C_SRCS += \
../Sources/bl_communication.c \
../Sources/main.c \
../Sources/sa_mtb.c \

OBJS += \
./Sources/bl_communication.o \
./Sources/main.o \
./Sources/sa_mtb.o \

OBJS_QUOTED += \
"./Sources/bl_communication.o" \
"./Sources/main.o" \
"./Sources/sa_mtb.o" \

C_DEPS += \
./Sources/bl_communication.d \
./Sources/main.d \
./Sources/sa_mtb.d \

OBJS_OS_FORMAT += \
./Sources/bl_communication.o \
./Sources/main.o \
./Sources/sa_mtb.o \

C_DEPS_QUOTED += \
"./Sources/bl_communication.d" \
"./Sources/main.d" \
"./Sources/sa_mtb.d" \


# Each subdirectory must supply rules for building sources it contributes
Sources/bl_communication.o: ../Sources/bl_communication.c
	@echo 'Building file: $<'
	@echo 'Executing target #1 $<'
	@echo 'Invoking: ARM Ltd Windows GCC C Compiler'
	"$(ARMSourceryDirEnv)/arm-none-eabi-gcc" "$<" @"Sources/bl_communication.args" -MMD -MP -MF"$(@:%.o=%.d)" -o"Sources/bl_communication.o"
	@echo 'Finished building: $<'
	@echo ' '

Sources/main.o: ../Sources/main.c
	@echo 'Building file: $<'
	@echo 'Executing target #2 $<'
	@echo 'Invoking: ARM Ltd Windows GCC C Compiler'
	"$(ARMSourceryDirEnv)/arm-none-eabi-gcc" "$<" @"Sources/main.args" -MMD -MP -MF"$(@:%.o=%.d)" -o"Sources/main.o"
	@echo 'Finished building: $<'
	@echo ' '

Sources/sa_mtb.o: ../Sources/sa_mtb.c
	@echo 'Building file: $<'
	@echo 'Executing target #3 $<'
	@echo 'Invoking: ARM Ltd Windows GCC C Compiler'
	"$(ARMSourceryDirEnv)/arm-none-eabi-gcc" "$<" @"Sources/sa_mtb.args" -MMD -MP -MF"$(@:%.o=%.d)" -o"Sources/sa_mtb.o"
	@echo 'Finished building: $<'
	@echo ' '


