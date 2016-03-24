function [  ] = breath_simons( zList, zFiltered, peaksPos, peaksHeight )
% Inställningar; välj dina inställnignar för koden här
%samplesPerSecBreath = 30;
% Slut inställningar


%% Andningens medelvärde över antal sekunder
%bpmBreath = (numberOfPeaksBreath/timeOfMeasurementBreath)*60;
% Slut medelvärde över antal sekunder

% Utskrifter
%meanBreath = round(bpmBreath);--------------------------meanBreath
% Slut utskrifter

% Plot av filtrerad data
h = figure('visible', 'off');
subplot(2,1,1)

plot(zList, 'blue');

subplot(2,1,2)
plot(zFiltered, 'red');
hold on
plot(peaksPos, peaksHeight, 'black o');
hold off

saveas(h, 'pulseplot.png')

end


