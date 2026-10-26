import type React from "react";

interface IsEmptyProps {
    title: string;
    subTitle?: string;
    icon: React.ElementType;
}

export const IsEmpty: React.FC<IsEmptyProps> = ({
    title,
    subTitle,
    icon: Icon
}) => {
    return (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-12 text-center">
            <Icon className="w-12 h-12 text-slate-300 mx-auto mb-3" />

            <p className="text-slate-600 font-medium">{title}</p>
            {subTitle && <p className="text-slate-400 text-sm">{subTitle}</p>}
        </div>
    );
};
