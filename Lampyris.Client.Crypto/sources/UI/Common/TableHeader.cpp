// Project Include(s)
#include "TableHeader.h"

TableHeaderDefinition& TableHeaderDefinition::startFieldGroup(double ratioOrWidth) {
    m_definition.push_back(FieldVector());
    if (!m_definition.back().empty()) {
        m_definition.back().back() = std::make_tuple("", false, ratioOrWidth);
    }
    else {
        m_definition.back().emplace_back("", false, ratioOrWidth);
    }
    return *this;
}

TableHeaderDefinition& TableHeaderDefinition::addField(const QString& fieldName, bool sortable) {
    if (!m_definition.empty()) {
        m_definition.back().emplace_back(fieldName, sortable, 0.0);
    }
    return *this;
}

void TableHeaderDefinition::end() {}

std::unordered_map<DataSortingOrder, QPixmap> TableHeader::ms_iconMap;

TableHeader::TableHeader(QWidget* parent)
    : QWidget(parent), m_layout(new QHBoxLayout(this)) {
    setStyleSheet("background-color: black;");
    m_layout->setContentsMargins(0, 0, 0, 0);
    m_layout->setSpacing(5);
    setLayout(m_layout);

    if (ms_iconMap.empty()) {
        ms_iconMap[DataSortingOrder::NONE] =
            QPixmap(":/res/icons/sort_none.png").scaled(16, 16, Qt::KeepAspectRatio, Qt::SmoothTransformation);
        ms_iconMap[DataSortingOrder::ASCENDING] = 
            QPixmap(":/res/icons/sort_ascending.png").scaled(16, 16, Qt::KeepAspectRatio, Qt::SmoothTransformation);
        ms_iconMap[DataSortingOrder::DESCENDING] = 
            QPixmap(":/res/icons/sort_descending.png").scaled(16, 16, Qt::KeepAspectRatio, Qt::SmoothTransformation);
    }
}

void TableHeader::setHeaderDefinition(const TableHeaderDefinition& definition) {
    // 清空现有布局
    QLayoutItem* child;
    while ((child = m_layout->takeAt(0)) != nullptr) {
        delete child->widget();
        delete child;
    }

    m_fieldInfoMap.clear();
    m_fieldGroupWidths.clear();

    // 遍历字段组
    for (const auto& fieldGroup : definition.m_definition) {
        createFieldGroup(fieldGroup);
    }
}

void TableHeader::createFieldGroup(const TableHeaderDefinition::FieldVector& fieldGroup) {
    double ratioOrWidth = std::get<2>(fieldGroup[0]);

    // 创建字段组容器
    QWidget* fieldGroupContainer = new QWidget(this);
    QHBoxLayout* fieldGroupLayout = new QHBoxLayout(fieldGroupContainer);
    fieldGroupLayout->setContentsMargins(0, 0, 0, 0);
    fieldGroupLayout->setSpacing(5);
    fieldGroupContainer->setLayout(fieldGroupLayout);

    fieldGroupLayout->addItem(new QSpacerItem(40, 20, QSizePolicy::Expanding, QSizePolicy::Preferred));
    for (size_t i = 1; i < fieldGroup.size(); ++i) {
        const QString& fieldName = std::get<0>(fieldGroup[i]);
        bool sortable = std::get<1>(fieldGroup[i]);

        // 创建字段容器
        QWidget* fieldContainer = new QWidget(fieldGroupContainer);
        QHBoxLayout* fieldLayout = new QHBoxLayout(fieldContainer);
        fieldLayout->setContentsMargins(0, 0, 0, 0);
        fieldLayout->setSpacing(0);
        fieldContainer->setLayout(fieldLayout);

        // 创建字段标签
        QLabel* fieldLabel = new QLabel(fieldName, fieldContainer);
        int fieldLabelWidth = fieldLabel->fontMetrics().horizontalAdvance(fieldName);
        fieldLabel->setStyleSheet("color: white;");
        fieldLabel->setAlignment(Qt::AlignCenter);

        // 创建箭头图标
        QLabel* arrowLabel = nullptr;
        if (sortable) {
            arrowLabel = new QLabel(fieldContainer);
            arrowLabel->setFixedSize(16, 16);
            arrowLabel->setPixmap(ms_iconMap[DataSortingOrder::NONE]);
        }

        // 创建斜线标签
        QLabel* lineLabel = (i < fieldGroup.size() - 1) ? new QLabel("/", fieldContainer) : nullptr;

        fieldContainer->setMaximumWidth(fieldLabelWidth + 20);

        FieldInfo info;
        info.fieldLabel = fieldLabel;
        info.arrowLabel = arrowLabel;
        info.sortable = sortable;
        info.sortOrder = DataSortingOrder::NONE;

        m_fieldInfoMap[fieldLabel] = info;

        // 安装事件过滤器
        fieldLabel->installEventFilter(this);

        // 将字段标签和箭头图标添加到字段容器
        fieldLayout->addWidget(fieldLabel);
        if (arrowLabel != nullptr) {
            fieldLayout->addWidget(arrowLabel);
        }
        if (lineLabel != nullptr) {
            lineLabel->setStyleSheet("color: white;");
            fieldLayout->addWidget(lineLabel);
        }

        // 将字段容器添加到字段组布局
        fieldGroupLayout->addWidget(fieldContainer);
    }
    fieldGroupLayout->addItem(new QSpacerItem(40, 20, QSizePolicy::Expanding, QSizePolicy::Preferred));

    // 存储字段组的宽度信息
    m_fieldGroupWidths.emplace_back(fieldGroupContainer, ratioOrWidth);

    // 将字段组容器添加到主布局
    m_layout->addWidget(fieldGroupContainer);
}

void TableHeader::adjustWidth() {
    int totalWidth = this->width();

    std::vector<TableColumnWidthInfo> widthInfoList(m_fieldGroupWidths.size());
    for (const auto& fieldGroup : m_fieldGroupWidths) {
        QWidget* fieldGroupContainer = fieldGroup.first;
        double ratioOrWidth = fieldGroup.second;

        if (ratioOrWidth > 0 && ratioOrWidth <= 1) {
            int fieldGroupWidth = static_cast<int>(totalWidth * ratioOrWidth);
            fieldGroupContainer->setFixedWidth(fieldGroupWidth);
            widthInfoList.emplace_back(fieldGroupWidth);
        }
        else if (ratioOrWidth > 1) {
            int width = static_cast<int>(ratioOrWidth);
            fieldGroupContainer->setFixedWidth(width);
            widthInfoList.emplace_back(width);
        }
    }

    emit columnWidthResized(widthInfoList);
}

void TableHeader::enterEvent(QEnterEvent* event) {
    QWidget::enterEvent(event);
}

void TableHeader::leaveEvent(QEvent* event) {
    QWidget::leaveEvent(event);
}

void TableHeader::resizeEvent(QResizeEvent* event) {
    // 避免递归触发
    QTimer::singleShot(0, this, [this]() {
        setMinimumSize(QSize(1, 1));
        setMaximumSize(QSize(3000, 3000));
        });
    adjustWidth();
    setFixedSize(event->size());
    QWidget::resizeEvent(event);
}

bool TableHeader::eventFilter(QObject* obj, QEvent* event) {
    QLabel* fieldLabel = qobject_cast<QLabel*>(obj);
    if (fieldLabel) {
        if (event->type() == QEvent::Enter) {
            fieldLabel->setStyleSheet("color: yellow;");
            return true;
        }
        else if (event->type() == QEvent::Leave) {
            fieldLabel->setStyleSheet("color: white;");
            return true;
        }
        else if (event->type() == QEvent::MouseButtonPress) {
            QMouseEvent* mouseEvent = static_cast<QMouseEvent*>(event);
            if (mouseEvent->button() == Qt::LeftButton) {
                auto it = m_fieldInfoMap.find(fieldLabel);
                if (it != m_fieldInfoMap.end() && it->second.sortable) {
                    DataSortingOrder sortOrder = static_cast<DataSortingOrder>((static_cast<int>(it->second.sortOrder) + 1) % 
                        static_cast<int>(DataSortingOrder::COUNT));
                    it->second.sortOrder = sortOrder;

                    // 取消掉旧的排序
                    if (m_sortingField != nullptr && m_sortingField != &(it->second)) {
                        m_sortingField->sortOrder = DataSortingOrder::NONE;
                        updateArrow(m_sortingField->fieldLabel, ms_iconMap[DataSortingOrder::NONE]);
                        m_sortingField = &(it->second);
                    }

                    updateArrow(fieldLabel, ms_iconMap[sortOrder] );

                    emit sortRequested(m_fieldInfoMap[fieldLabel].fieldLabel->text(), sortOrder);
                    return true;
                }
            }
        }
    }
    return QWidget::eventFilter(obj, event);
}

void TableHeader::updateArrow(QLabel* fieldLabel, const QPixmap& pixmap) {
    auto it = m_fieldInfoMap.find(fieldLabel);
    if (it != m_fieldInfoMap.end()) {
        it->second.arrowLabel->setPixmap(pixmap);
    }
}