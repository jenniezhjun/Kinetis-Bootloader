################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../Driver/rs232/rs232.c 

OBJS += \
./Driver/rs232/rs232.o 

C_DEPS += \
./Driver/rs232/rs232.d 


# Each subdirectory must supply rules for building sources it contributes
Driver/rs232/%.o: ../Driver/rs232/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: Cross ARM C Compiler'
	arm-none-eabi-gcc -mcpu=cortex-m4 -mthumb -mfloat-abi=hard -mfpu=fpv4-sp-d16 -Os -fmessage-length=0 -fsigned-char -ffunction-sections -fdata-sections  -g3 -I"../Sources" -I"../Driver/rs232" -I"../Driver/flash" -I"../Driver/bootcfg" -I"../Includes" -std=c99 -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


