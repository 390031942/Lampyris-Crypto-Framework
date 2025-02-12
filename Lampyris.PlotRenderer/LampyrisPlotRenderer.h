#pragma once

#include <QtWidgets/QMainWindow>
#include "ui_LampyrisPlotRenderer.h"

class LampyrisPlotRenderer : public QMainWindow {
    Q_OBJECT
public:
    LampyrisPlotRenderer(QWidget *parent = nullptr);
    ~LampyrisPlotRenderer();
private:
    Ui::LampyrisPlotRendererClass ui;
};
