#pragma once
// QT Include(s)
#include <QWidget>
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QSpacerItem>
#include <QPushButton>
#include <QLabel>
#include <QLineEdit>
#include <QCheckBox>
#include <QRadioButton>
#include <QComboBox>
#include <QSlider>
#include <QSpinBox>
#include <QDoubleSpinBox>
#include <QProgressBar>
#include <QPixmap>
#include <QLayout>

// STD Include(s)
#include <stack>

// Project Include(s)
#include "UI/Common/BetterLineEdit.h"

class LayoutBuilder {
public:
    explicit LayoutBuilder(QWidget* targetWidget)
        : m_targetWidget(targetWidget) {
        // 默认使用一个 QVBoxLayout 作为主布局
        QVBoxLayout* mainLayout = new QVBoxLayout(targetWidget);
        mainLayout->setContentsMargins(10, 0, 10, 0);
        mainLayout->setSpacing(6);
        m_targetWidget->setLayout(mainLayout);
        m_layoutStack.push(mainLayout); // 将主布局压入栈中
    }

    // 开始一个水平布局，返回布局的容器 QWidget
    QWidget* beginHorizontalLayout() {
        QWidget* container = new QWidget(m_targetWidget); // 创建一个新的容器 QWidget
        QHBoxLayout* hLayout = new QHBoxLayout(container); // 创建水平布局并设置为容器的布局
        hLayout->setContentsMargins(0, 0, 0, 0);
        currentLayout()->addWidget(container); // 将容器添加到当前布局中
        m_layoutStack.push(hLayout);          // 将新布局压入栈中
        return container;                     // 返回容器
    }

    // 开始一个垂直布局，返回布局的容器 QWidget
    QWidget* beginVerticalLayout() {
        QWidget* container = new QWidget(m_targetWidget); // 创建一个新的容器 QWidget
        QVBoxLayout* vLayout = new QVBoxLayout(container); // 创建垂直布局并设置为容器的布局
        vLayout->setContentsMargins(0, 0, 0, 0);
        currentLayout()->addWidget(container); // 将容器添加到当前布局中
        m_layoutStack.push(vLayout);          // 将新布局压入栈中
        return container;                     // 返回容器
    }

    // 结束当前布局
    void endLayout() {
        if (m_layoutStack.size() > 1) { // 确保主布局不会被弹出
            m_layoutStack.pop();
        }
    }

    // 添加 QSpacerItem
    void addSpacerItem(int widthOrHeight = 20) {
        QSpacerItem* spacer = nullptr;
        QBoxLayout* layout = currentLayout();

        // 根据栈顶布局类型设置 SpacerItem 的尺寸策略
        if (dynamic_cast<QHBoxLayout*>(layout)) {
            // 如果是水平布局，设置宽度为扩展，高度为固定
            spacer = new QSpacerItem(widthOrHeight, 0, QSizePolicy::Expanding, QSizePolicy::Fixed);
        }
        else if (dynamic_cast<QVBoxLayout*>(layout)) {
            // 如果是垂直布局，设置高度为扩展，宽度为固定
            spacer = new QSpacerItem(0, widthOrHeight, QSizePolicy::Fixed, QSizePolicy::Expanding);
        }

        if (spacer) {
            layout->addItem(spacer);
        }
    }

    QFrame* addSplitLine() {
        QFrame* line = new QFrame(m_targetWidget);
        QBoxLayout* layout = currentLayout();

        // 根据栈顶布局类型设置分割线的方向和样式
        if (dynamic_cast<QHBoxLayout*>(layout)) {
            line->setFrameShape(QFrame::VLine);
            line->setSizePolicy(QSizePolicy::Fixed, QSizePolicy::Expanding);
            line->setFixedWidth(1);
        }
        else if (dynamic_cast<QVBoxLayout*>(layout)) {
            line->setFrameShape(QFrame::HLine);
            line->setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Fixed);
            line->setFixedHeight(1);
        }

        line->setFrameShape(QFrame::HLine);
        line->setFrameShadow(QFrame::Plain);
        line->setStyleSheet("color: white;"); 
        layout->addWidget(line);

        return line;
    }

    // 添加 QLabel
    QLabel* addLabel(const QString& text = QString()) {
        QLabel* label = new QLabel(text, m_targetWidget);
        currentLayout()->addWidget(label);
        return label;
    }

    // 添加 QPushButton
    QPushButton* addButton(const QString& text) {
        QPushButton* button = new QPushButton(text, m_targetWidget);
        currentLayout()->addWidget(button);
        button->setFocusPolicy(Qt::NoFocus);
        return button;
    }

    QPushButton* addIconButton(const QString& iconPath, const QSize& size = QSize(32, 32)) {
        QPushButton* button = new QPushButton(m_targetWidget);
        QIcon icon(iconPath);
        button->setIcon(icon);
        button->setIconSize(size); // 设置图标大小
        button->setFocusPolicy(Qt::NoFocus);
        currentLayout()->addWidget(button);
        return button;
    }

    // 添加 QLineEdit
    BetterLineEdit* addLineEdit(const QString& placeholderText = QString(), QStringList options = QStringList()) {
        BetterLineEdit* lineEdit = new BetterLineEdit(m_targetWidget);
        lineEdit->setPlaceholderText(placeholderText);
        lineEdit->setOptions(options);
        currentLayout()->addWidget(lineEdit);
        return lineEdit;
    }

    // 添加 QCheckBox
    QCheckBox* addCheckBox(const QString& text) {
        QCheckBox* checkBox = new QCheckBox(text, m_targetWidget);
        currentLayout()->addWidget(checkBox);
        return checkBox;
    }

    // 添加 QRadioButton
    QRadioButton* addRadioButton(const QString& text) {
        QRadioButton* radioButton = new QRadioButton(text, m_targetWidget);
        currentLayout()->addWidget(radioButton);
        return radioButton;
    }

    // 添加 QComboBox
    QComboBox* addComboBox(const QStringList& items = QStringList()) {
        QComboBox* comboBox = new QComboBox(m_targetWidget);
        comboBox->addItems(items);
        currentLayout()->addWidget(comboBox);
        return comboBox;
    }

    // 添加 QSlider
    QSlider* addSlider(Qt::Orientation orientation = Qt::Horizontal) {
        QSlider* slider = new QSlider(orientation, m_targetWidget);
        currentLayout()->addWidget(slider);
        return slider;
    }

    // 添加 QSpinBox
    QSpinBox* addSpinBox(int min = 0, int max = 100, int value = 0) {
        QSpinBox* spinBox = new QSpinBox(m_targetWidget);
        spinBox->setRange(min, max);
        spinBox->setValue(value);
        currentLayout()->addWidget(spinBox);
        return spinBox;
    }

    // 添加 QDoubleSpinBox
    QDoubleSpinBox* addDoubleSpinBox(double min = 0.0, double max = 100.0, double value = 0.0) {
        QDoubleSpinBox* doubleSpinBox = new QDoubleSpinBox(m_targetWidget);
        doubleSpinBox->setRange(min, max);
        doubleSpinBox->setValue(value);
        currentLayout()->addWidget(doubleSpinBox);
        return doubleSpinBox;
    }

    // 添加 QProgressBar
    QProgressBar* addProgressBar(int min = 0, int max = 100, int value = 0) {
        QProgressBar* progressBar = new QProgressBar(m_targetWidget);
        progressBar->setRange(min, max);
        progressBar->setValue(value);
        currentLayout()->addWidget(progressBar);
        return progressBar;
    }

    // 添加图片 QLabel
    QLabel* addImage(const QString& pixmapPath) {
        QPixmap pixmap(pixmapPath);
        QLabel* label = new QLabel(m_targetWidget);
        label->setPixmap(pixmap);
        currentLayout()->addWidget(label);
        return label;
    }

    void addWidget(QWidget* widget) {
        currentLayout()->addWidget(widget);
    }
private:
    QWidget* m_targetWidget;               // 目标 QWidget
    std::stack<QBoxLayout*> m_layoutStack; // 布局栈

    // 获取当前布局
    QBoxLayout* currentLayout() {
        return m_layoutStack.top();
    }
};
