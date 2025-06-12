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
        // Ĭ��ʹ��һ�� QVBoxLayout ��Ϊ������
        QVBoxLayout* mainLayout = new QVBoxLayout(targetWidget);
        mainLayout->setContentsMargins(10, 0, 10, 0);
        mainLayout->setSpacing(6);
        m_targetWidget->setLayout(mainLayout);
        m_layoutStack.push(mainLayout); // ��������ѹ��ջ��
    }

    // ��ʼһ��ˮƽ���֣����ز��ֵ����� QWidget
    QWidget* beginHorizontalLayout() {
        QWidget* container = new QWidget(m_targetWidget); // ����һ���µ����� QWidget
        QHBoxLayout* hLayout = new QHBoxLayout(container); // ����ˮƽ���ֲ�����Ϊ�����Ĳ���
        hLayout->setContentsMargins(0, 0, 0, 0);
        currentLayout()->addWidget(container); // ��������ӵ���ǰ������
        m_layoutStack.push(hLayout);          // ���²���ѹ��ջ��
        return container;                     // ��������
    }

    // ��ʼһ����ֱ���֣����ز��ֵ����� QWidget
    QWidget* beginVerticalLayout() {
        QWidget* container = new QWidget(m_targetWidget); // ����һ���µ����� QWidget
        QVBoxLayout* vLayout = new QVBoxLayout(container); // ������ֱ���ֲ�����Ϊ�����Ĳ���
        vLayout->setContentsMargins(0, 0, 0, 0);
        currentLayout()->addWidget(container); // ��������ӵ���ǰ������
        m_layoutStack.push(vLayout);          // ���²���ѹ��ջ��
        return container;                     // ��������
    }

    // ������ǰ����
    void endLayout() {
        if (m_layoutStack.size() > 1) { // ȷ�������ֲ��ᱻ����
            m_layoutStack.pop();
        }
    }

    // ��� QSpacerItem
    void addSpacerItem(int widthOrHeight = 20) {
        QSpacerItem* spacer = nullptr;
        QBoxLayout* layout = currentLayout();

        // ����ջ�������������� SpacerItem �ĳߴ����
        if (dynamic_cast<QHBoxLayout*>(layout)) {
            // �����ˮƽ���֣����ÿ��Ϊ��չ���߶�Ϊ�̶�
            spacer = new QSpacerItem(widthOrHeight, 0, QSizePolicy::Expanding, QSizePolicy::Fixed);
        }
        else if (dynamic_cast<QVBoxLayout*>(layout)) {
            // ����Ǵ�ֱ���֣����ø߶�Ϊ��չ�����Ϊ�̶�
            spacer = new QSpacerItem(0, widthOrHeight, QSizePolicy::Fixed, QSizePolicy::Expanding);
        }

        if (spacer) {
            layout->addItem(spacer);
        }
    }

    QFrame* addSplitLine() {
        QFrame* line = new QFrame(m_targetWidget);
        QBoxLayout* layout = currentLayout();

        // ����ջ�������������÷ָ��ߵķ������ʽ
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

    // ��� QLabel
    QLabel* addLabel(const QString& text = QString()) {
        QLabel* label = new QLabel(text, m_targetWidget);
        currentLayout()->addWidget(label);
        return label;
    }

    // ��� QPushButton
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
        button->setIconSize(size); // ����ͼ���С
        button->setFocusPolicy(Qt::NoFocus);
        currentLayout()->addWidget(button);
        return button;
    }

    // ��� QLineEdit
    BetterLineEdit* addLineEdit(const QString& placeholderText = QString(), QStringList options = QStringList()) {
        BetterLineEdit* lineEdit = new BetterLineEdit(m_targetWidget);
        lineEdit->setPlaceholderText(placeholderText);
        lineEdit->setOptions(options);
        currentLayout()->addWidget(lineEdit);
        return lineEdit;
    }

    // ��� QCheckBox
    QCheckBox* addCheckBox(const QString& text) {
        QCheckBox* checkBox = new QCheckBox(text, m_targetWidget);
        currentLayout()->addWidget(checkBox);
        return checkBox;
    }

    // ��� QRadioButton
    QRadioButton* addRadioButton(const QString& text) {
        QRadioButton* radioButton = new QRadioButton(text, m_targetWidget);
        currentLayout()->addWidget(radioButton);
        return radioButton;
    }

    // ��� QComboBox
    QComboBox* addComboBox(const QStringList& items = QStringList()) {
        QComboBox* comboBox = new QComboBox(m_targetWidget);
        comboBox->addItems(items);
        currentLayout()->addWidget(comboBox);
        return comboBox;
    }

    // ��� QSlider
    QSlider* addSlider(Qt::Orientation orientation = Qt::Horizontal) {
        QSlider* slider = new QSlider(orientation, m_targetWidget);
        currentLayout()->addWidget(slider);
        return slider;
    }

    // ��� QSpinBox
    QSpinBox* addSpinBox(int min = 0, int max = 100, int value = 0) {
        QSpinBox* spinBox = new QSpinBox(m_targetWidget);
        spinBox->setRange(min, max);
        spinBox->setValue(value);
        currentLayout()->addWidget(spinBox);
        return spinBox;
    }

    // ��� QDoubleSpinBox
    QDoubleSpinBox* addDoubleSpinBox(double min = 0.0, double max = 100.0, double value = 0.0) {
        QDoubleSpinBox* doubleSpinBox = new QDoubleSpinBox(m_targetWidget);
        doubleSpinBox->setRange(min, max);
        doubleSpinBox->setValue(value);
        currentLayout()->addWidget(doubleSpinBox);
        return doubleSpinBox;
    }

    // ��� QProgressBar
    QProgressBar* addProgressBar(int min = 0, int max = 100, int value = 0) {
        QProgressBar* progressBar = new QProgressBar(m_targetWidget);
        progressBar->setRange(min, max);
        progressBar->setValue(value);
        currentLayout()->addWidget(progressBar);
        return progressBar;
    }

    // ���ͼƬ QLabel
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
    QWidget* m_targetWidget;               // Ŀ�� QWidget
    std::stack<QBoxLayout*> m_layoutStack; // ����ջ

    // ��ȡ��ǰ����
    QBoxLayout* currentLayout() {
        return m_layoutStack.top();
    }
};
