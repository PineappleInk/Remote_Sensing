function [ meanBreath ] = breath( zList )
% Calculates mean number of breaths over some time-interval

% Inställningar; välj dina inställnignar för koden här
samplesPerSecBreath = 30;
% Slut inställningar

%% Info om mätdata
numberOfSamplesBreath = length(zList);
timeOfMeasurementBreath = numberOfSamplesBreath/30;
% Slut på info om mätdata

% Gör längden på vectorns indata udda
if (mod(numberOfSamplesBreath,2) == 0) % color_list är jämn i längd
    zList = zList(2:numberOfSamplesBreath);
    numberOfSamplesBreath = length(zList);
end
% Slut

%% Kontrollerar att listan är tillräckligt lång
if length(zList)<4
    error('Not enough samples of breath')
end
%Slut kontroll

%% Filtrera zList med hjälp av högsta möjliga gradens
% Savitzky Golay FIR-filter
zList=double(zList);

zList=double(zList); % Kanske ej behövs
degreeOfPolynomialBreath = samplesPerSecBreath - 1;
smoothZList = sgolayfilt(zList,degreeOfPolynomialBreath,numberOfSamplesBreath);
% Vänd på kurvan för mer naturligt gränssnitt och beräkning av
% inandnignar
smoothZList = smoothZList.*(-1);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaksBreath, peakLocationBreath]=findpeaks(smoothZList);
numberOfPeaksBreath=length(peakLocationBreath);
% Slut filtrering

%% Andningens medelvärde över antal sekunder
bpmBreath = (numberOfPeaksBreath/timeOfMeasurementBreath)*60;
% Slut medelvärde över antal sekunder

% Utskrifter
meanBreath = bpmBreath
% Slut utskrifter

% Plot av filtrerad data
figure('visible', 'off')
subplot(2,1,2)
hold off
plot(smoothZList, 'blue' );
hold on
plot(peakLocationBreath, heightOfPeaksBreath, 'blue o');
grid on
title({'Andetag per minut:', meanBreath}, 'color', 'blue', 'FontWeight', 'bold')
xlabel('tid [s/30]')
ylabel('Andningskurva')
% Slut plot

end


