function [ mean_pulse ] = pulse_instant( color_list )
% Calculates mean pulse over some time-interval

% Inställningar; välj dina inställnignar för koden här
samples_per_sec = 30;
% Slut inställningar

% Info om mätdata
number_of_samples = length(color_list);
time_of_measurement = number_of_samples/samples_per_sec;
% Slut på info om mätdata

% Gör längden på vectorns indata udda
if (mod(number_of_samples,2) == 0) % color_list är jämn i längd
    color_list = color_list(2:number_of_samples);
    number_of_samples = length(color_list);
end
% Slut

% Plot av rådata
figure(2)
subplot(2,1,1)
plot(color_list);
grid on
%grid minor
title('Plot puls rådata');
xlabel('tid [s/30]')
ylabel('Färgskillnad i pixel i ansiktet (röd-grön)')

% Filtrera color_list med hjälp av högsta möjliga gradens
% Savitzky Golay FIR-filter
if length(color_list)>=3
    
    color_list=double(color_list); % Kanske ej behövs
    degree_of_polynomial = samples_per_sec - 1;
    smooth_color_list = sgolayfilt(color_list,degree_of_polynomial,number_of_samples);
    
    % Lokaliserar peakarna i den filtrerade kurvan
    %[~, peak_location]=findpeaks(smooth_color_list, 'MinPeakDistance',10);
    [~, peak_location]=findpeaks(smooth_color_list)
    number_of_peaks=length(peak_location);
end
% Slut filtrering

% Pulsens medelvärde över antal sekunder
BPM_pulse = (number_of_peaks/time_of_measurement)*60;
% Slut medelvärde över antal sekunder

% Plot av filtrerad data
%figure(2)
subplot(2,1,2)
plot(smooth_color_list);
grid on
%grid minor
title('Plot puls filtrerad med Savitzky Golay av högsta möjliga grad');
xlabel('tid [s/30]')
ylabel('Färgskillnad i pixel i ansiktet (röd-grön)')
% Slut plot av filtrerad data

% Utskrifter
mean_pulse = BPM_pulse
% Slut utskrifter

end

