using System;
using System.Reflection;
using System.Reflection.Emit;

using GetText.Plural.Ast;

namespace GetText.PluralCompile.Compiler
{
    /// <summary>
    /// Compiler that compiles a plural rule abstract syntax tree 
    /// into a managed dynamic method delegate using an IL code generator.
    /// </summary>
    public class PluralRuleCompiler
    {
        public const string DynamicMethodName = "CompiledPluralRuleDynamicMethod";
        public const string InvokeMethodName = "Invoke";

        /// <summary>
        /// Compiles a plural rule abstract syntax tree into managed dynamic method delegate using an IL code generator.
        /// </summary>
        /// <param name="astRoot">abstract syntax tree root node.</param>
        /// <param name="outputDelegateType">Type of output delegate.</param>
        /// <returns>Compiled dynamic method of given type.</returns>
        public virtual Delegate CompileToDynamicMethod(Token astRoot, Type outputDelegateType)
        {
            DynamicMethod dynamicMethod = CreateDynamicMethod(outputDelegateType);
            ILGenerator il = dynamicMethod.GetILGenerator();

            CompileStart(il);
            CompileNode(il, astRoot);
            CompileFinish(il);

            return dynamicMethod.CreateDelegate(outputDelegateType);
        }

        /// <summary>
        /// Creates a new <see cref="DynamicMethod"/> instance with signature of given delegate type.
        /// </summary>
        /// <param name="outputDelegateType"></param>
        /// <returns></returns>
        protected virtual DynamicMethod CreateDynamicMethod(Type outputDelegateType)
        {
            MethodInfo methodInfo = outputDelegateType?.GetMethod(InvokeMethodName);

            ParameterInfo[] parameters = methodInfo.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterTypes[i] = parameters[i].ParameterType;
            }

            return new DynamicMethod(DynamicMethodName, methodInfo.ReturnType, parameterTypes);
        }

        /// <summary>
        /// Emits instructions required for compilation start procedure.
        /// </summary>
        /// <param name="il">IL generator instance.</param>
        protected virtual void CompileStart(ILGenerator il)
        {
        }

        /// <summary>
        /// Emits instructions required for compilation stop procedure.
        /// </summary>
        /// <param name="il">IL generator instance.</param>
        protected virtual void CompileFinish(ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Conv_I4); // Convert our Int64 value on the stack to Int32
            il.Emit(OpCodes.Ret); // Return value from the stack
        }

        /// <summary>
        /// Recursively compiles an AST node to the IL instructions.
        /// </summary>
        /// <param name="il">IL generator instance.</param>
        /// <param name="node">AST node.</param>
        protected virtual void CompileNode(ILGenerator il, Token node)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            switch (node.Type)
            {
                case TokenType.Number:
                    il.Emit(OpCodes.Ldc_I8, node.Value);
                    break;

                case TokenType.N:
                    il.Emit(OpCodes.Ldarg_0);
                    break;

                case TokenType.Plus:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    il.Emit(OpCodes.Add);
                    break;

                case TokenType.Minus:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    il.Emit(OpCodes.Sub);
                    break;

                case TokenType.Divide:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    il.Emit(OpCodes.Div);
                    break;

                case TokenType.Multiply:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    il.Emit(OpCodes.Mul);
                    break;

                case TokenType.Modulo:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    il.Emit(OpCodes.Rem);
                    break;

                case TokenType.GreaterThan:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    EmitConditionalValue(il, OpCodes.Bgt);
                    break;

                case TokenType.GreaterOrEquals:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    EmitConditionalValue(il, OpCodes.Bge);
                    break;

                case TokenType.LessThan:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    EmitConditionalValue(il, OpCodes.Blt);
                    break;

                case TokenType.LessOrEquals:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    EmitConditionalValue(il, OpCodes.Ble);
                    break;

                case TokenType.Equals:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    EmitConditionalValue(il, OpCodes.Beq);
                    break;

                case TokenType.NotEquals:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    EmitConditionalValue(il, OpCodes.Beq, 0, 1);
                    break;

                case TokenType.And:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    il.Emit(OpCodes.And);
                    break;

                case TokenType.Or:
                    CompileNode(il, node.Children[0]);
                    CompileNode(il, node.Children[1]);
                    il.Emit(OpCodes.Or);
                    break;

                case TokenType.Not:
                    CompileNode(il, node.Children[0]);
                    EmitConditionalValue(il, OpCodes.Brfalse);
                    break;

                case TokenType.TernaryIf:
                    CompileNode(il, node.Children[0]);
                    EmitConditionalBranch(il, OpCodes.Brtrue, node.Children[1], node.Children[2]);
                    break;
            }
        }

        /// <summary>
        /// Emits instructions required for conditional branch using given OpCode to check condition and given nodes for positive and negative branches.
        /// </summary>
        /// <param name="il">IL generator instance.</param>
        /// <param name="conditionOpCode">OpCode of the condition check operation.</param>
        /// <param name="trueNode">AST node that will be executed when condition returns positive result.</param>
        /// <param name="falseNode">AST node that will be executed when condition returns negative result.</param>
        protected virtual void EmitConditionalBranch(ILGenerator il, OpCode conditionOpCode, Token trueNode, Token falseNode)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            Label trueLabel = il.DefineLabel();
            Label endLabel = il.DefineLabel();

            il.Emit(conditionOpCode, trueLabel);
            CompileNode(il, falseNode);
            il.Emit(OpCodes.Br, endLabel);

            il.MarkLabel(trueLabel);
            CompileNode(il, trueNode);

            il.MarkLabel(endLabel);
        }

        /// <summary>
        /// Emits instructions required for creating a value on the stack based on specified condition operation result.
        /// </summary>
        /// <remarks>
        /// We can not use simplified <see cref="OpCodes.Clt"/>, <see cref="OpCodes.Cgt"/> and similar
        /// instructions because we use an <see cref="long"/> as an argument value.
        /// </remarks>
        /// <param name="il">IL generator instance.</param>
        /// <param name="conditionOpCode">OpCode of the condition check operation.</param>
        /// <param name="trueValue">A value that will be put on stack when condition returns positive result.</param>
        /// <param name="falseValue">A value that will be put on stack when condition returns negative result.</param>
        protected virtual void EmitConditionalValue(ILGenerator il, OpCode conditionOpCode, long trueValue = 1, long falseValue = 0)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            Label trueLabel = il.DefineLabel();
            Label endLabel = il.DefineLabel();

            il.Emit(conditionOpCode, trueLabel);
            il.Emit(OpCodes.Ldc_I8, falseValue);
            il.Emit(OpCodes.Br, endLabel);

            il.MarkLabel(trueLabel);
            il.Emit(OpCodes.Ldc_I8, trueValue);

            il.MarkLabel(endLabel);
        }
    }
}
