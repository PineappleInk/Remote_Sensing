function [ mean_pulse ] = pulse_instant( color_list )
% Calculates mean pulse over some time-interval
% clf(subplot(2,2,1))
% clf(subplot(2,2,3))

% Inst�llningar; v�lj dina inst�llnignar f�r koden h�r
samples_per_sec = 30;
% Slut inst�llningar

% Info om m�tdata
number_of_samples = length(color_list);
time_of_measurement = number_of_samples/samples_per_sec;
% Slut p� info om m�tdata

% G�r l�ngden p� vectorns indata udda
if (mod(number_of_samples,2) == 0) % color_list �r j�mn i l�ngd
    color_list = color_list(2:number_of_samples);
    number_of_samples = length(color_list);
end
% Slut

% Plot av r�data
figure(1)
subplot(2,2,1)
plot(color_list, 'red');
grid on
%grid minor
title('Plot puls r�data');
xlabel('tid [s/30]')
ylabel('F�rgskillnad i pixel i ansiktet (r�d-gr�n)')
% Slut plot r�data

% Filtrera color_list med hj�lp av h�gsta m�jliga gradens
% Savitzky Golay FIR-filter
if length(color_list)>=3
    
    color_list=double(color_list); % Kanske ej beh�vs
    degree_of_polynomial = samples_per_sec - 1;
    smooth_color_list = sgolayfilt(color_list,degree_of_polynomial,number_of_samples);
    
    % Lokaliserar peakarna i den filtrerade kurvan
    %[~, peak_location]=findpeaks(smooth_color_list, 'MinPeakDistance',10);
    [height_of_peaks, peak_location]=findpeaks(smooth_color_list);
    number_of_peaks=length(peak_location);
end
% Slut filtrering

% Pulsens medelv�rde �ver antal sekunder
BPM_pulse = (number_of_peaks/time_of_measurement)*60;
% Slut medelv�rde �ver antal sekunder

% Plot av filtrerad data
subplot(2,2,3)
plot(smooth_color_list, 'red');
% hold on
% plot(peak_location, height_of_peaks, 'red o');
grid on
title('Plot puls filtrerad med Savitzky Golay av h�gsta m�jliga grad');
xlabel('tid [s/30]')
ylabel('F�rgskillnad i pixel i ansiktet (r�d-gr�n)')
%hold off
% Slut plot av filtrerad data

% Utskrifter
mean_pulse = BPM_pulse
% Slut utskrifter

end