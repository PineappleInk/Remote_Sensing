function [  ] = breath_simons( zList, zFiltered, peaksPos, peaksHeight )
% Inst�llningar; v�lj dina inst�llnignar f�r koden h�r
%samplesPerSecBreath = 30;
% Slut inst�llningar


%% Andningens medelv�rde �ver antal sekunder
%bpmBreath = (numberOfPeaksBreath/timeOfMeasurementBreath)*60;
% Slut medelv�rde �ver antal sekunder

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


