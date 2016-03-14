function [ mean_breath ] = breathing_instant( z_list )
% % Calculates mean number of breaths over some time-interval
% clf(subplot(2,2,2))
% clf(subplot(2,2,4))

% Inställningar; välj dina inställnignar för koden här
samples_per_sec = 30;
% Slut inställningar

% Info om mätdata
number_of_samples = length(z_list);
time_of_measurement = number_of_samples/30;
% Slut på info om mätdata

% Gör längden på vectorns indata udda
if (mod(number_of_samples,2) == 0) % color_list är jämn i längd
    z_list = z_list(2:number_of_samples);
    number_of_samples = length(z_list);
end
% Slut

% Plot av rådata
figure(1)
subplot(2,2,2)
plot(z_list,'blue');
grid on
%grid minor
title('Plot andningsfrekvens');
xlabel('tid [s/30]')
ylabel('Djupskillnad i bröst')
% Slut plot

% Filtrera z_list med hjälp av högsta möjliga gradens
% Savitzky Golay FIR-filter
if length(z_list)>=3
    z_list=double(z_list);
    
    z_list=double(z_list); % Kanske ej behövs
    degree_of_polynomial = samples_per_sec - 1;
    smooth_z_list = sgolayfilt(z_list,degree_of_polynomial,number_of_samples);
    
    % Lokaliserar peakarna i den filtrerade kurvan
    [height_of_peaks, peak_location]=findpeaks(smooth_z_list);
    number_of_peaks=length(peak_location);
end
% Slut filtrering

% Andningens medelvärde över antal sekunder
BPM_breath = (number_of_peaks/time_of_measurement)*60;
% Slut medelvärde över antal sekunder

% Plot av filtrerad data
subplot(2,2,4)
plot(smooth_z_list, 'blue' );
% hold on
% plot(peak_location, height_of_peaks, 'blue o');
grid on
title('Plot andning filtrerad med Savitzky Golay av högsta möjliga grad');
xlabel('tid [s/30]')
ylabel('Andning')
%hold off
% Slut plot

% Utskrifter
mean_breath = BPM_breath
% Slut utskrifter

end

